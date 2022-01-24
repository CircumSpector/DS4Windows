using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using DS4Windows.Shared.Common.Legacy;

namespace DS4Windows
{
    public enum DsState : byte
    {
        [Description("Disconnected")] Disconnected = 0x00,
        [Description("Reserved")] Reserved = 0x01,
        [Description("Connected")] Connected = 0x02
    }

    public enum DsConnection : byte
    {
        [Description("None")] None = 0x00,
        [Description("Usb")] Usb = 0x01,
        [Description("Bluetooth")] Bluetooth = 0x02
    }

    public enum DsModel : byte
    {
        [Description("None")] None = 0,
        [Description("DualShock 3")] DS3 = 1,
        [Description("DualShock 4")] DS4 = 2,
        [Description("Generic Gamepad")] Generic = 3
    }

    public enum DsBattery : byte
    {
        None = 0x00,
        Dying = 0x01,
        Low = 0x02,
        Medium = 0x03,
        High = 0x04,
        Full = 0x05,
        Charging = 0xEE,
        Charged = 0xEF
    }

    public struct DualShockPadMeta
    {
        public byte PadId;
        public DsState PadState;
        public DsConnection ConnectionType;
        public DsModel Model;
        public PhysicalAddress PadMacAddress;
        public DsBattery BatteryStatus;
        public bool IsActive;
    }

    internal class UdpServer
    {
        public delegate void GetPadDetail(int padIdx, ref DualShockPadMeta meta);

        public const int NUMBER_SLOTS = 4;
        private const int ARG_BUFFER_LEN = 80;

        private const ushort MaxProtocolVersion = 1001;
        private readonly SemaphoreSlim _pool;
        private readonly SocketAsyncEventArgs[] argsList;

        private readonly Dictionary<IPEndPoint, ClientRequestTimes> clients = new();
        private int listInd;
        private readonly ReaderWriterLockSlim poolLock = new();

        private readonly GetPadDetail portInfoGet;
        private readonly byte[] recvBuffer = new byte[1024];
        private bool running;
        private uint serverId;
        private Socket udpSock;

        public UdpServer(GetPadDetail getPadDetailDel)
        {
            portInfoGet = getPadDetailDel;
            _pool = new SemaphoreSlim(ARG_BUFFER_LEN);
            argsList = new SocketAsyncEventArgs[ARG_BUFFER_LEN];
            for (var num = 0; num < ARG_BUFFER_LEN; num++)
            {
                var args = new SocketAsyncEventArgs();
                args.SetBuffer(new byte[100], 0, 100);
                args.Completed += SocketEvent_Completed;
                argsList[num] = args;
            }
        }

        private void SocketEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            _pool.Release();
        }

        private void CompletedSynchronousSocketEvent()
        {
            _pool.Release();
        }

        private int BeginPacket(byte[] packetBuf, ushort reqProtocolVersion = MaxProtocolVersion)
        {
            var currIdx = 0;
            packetBuf[currIdx++] = (byte)'D';
            packetBuf[currIdx++] = (byte)'S';
            packetBuf[currIdx++] = (byte)'U';
            packetBuf[currIdx++] = (byte)'S';

            Array.Copy(BitConverter.GetBytes(reqProtocolVersion), 0, packetBuf, currIdx, 2);
            currIdx += 2;

            Array.Copy(BitConverter.GetBytes((ushort)packetBuf.Length - 16), 0, packetBuf, currIdx, 2);
            currIdx += 2;

            Array.Clear(packetBuf, currIdx, 4); //place for crc
            currIdx += 4;

            Array.Copy(BitConverter.GetBytes(serverId), 0, packetBuf, currIdx, 4);
            currIdx += 4;

            return currIdx;
        }

        private void FinishPacket(byte[] packetBuf)
        {
            Array.Clear(packetBuf, 8, 4);

            //uint crcCalc = Crc32Algorithm.Compute(packetBuf);
            var seed = Crc32Algorithm.DefaultSeed;
            var crcCalc = ~Crc32Algorithm.CalculateBasicHash(ref seed, ref packetBuf, 0, packetBuf.Length);
            Array.Copy(BitConverter.GetBytes(crcCalc), 0, packetBuf, 8, 4);
        }

        private void SendPacket(IPEndPoint clientEP, byte[] usefulData, ushort reqProtocolVersion = MaxProtocolVersion)
        {
            var packetData = new byte[usefulData.Length + 16];
            var currIdx = BeginPacket(packetData, reqProtocolVersion);
            Array.Copy(usefulData, 0, packetData, currIdx, usefulData.Length);
            FinishPacket(packetData);

            //try { udpSock.SendTo(packetData, clientEP); }
            var temp = 0;
            poolLock.EnterWriteLock();
            temp = listInd;
            listInd = ++listInd % ARG_BUFFER_LEN;
            var args = argsList[temp];
            poolLock.ExitWriteLock();

            _pool.Wait();
            args.RemoteEndPoint = clientEP;
            Array.Copy(packetData, args.Buffer, packetData.Length);
            //args.SetBuffer(packetData, 0, packetData.Length);
            var sentAsync = false;
            try
            {
                sentAsync = udpSock.SendToAsync(args);
                if (!sentAsync) CompletedSynchronousSocketEvent();
            }
            catch (Exception /*e*/)
            {
            }
            finally
            {
                if (!sentAsync) CompletedSynchronousSocketEvent();
            }
        }

        private void ProcessIncoming(byte[] localMsg, IPEndPoint clientEP)
        {
            try
            {
                var currIdx = 0;
                if (localMsg[0] != 'D' || localMsg[1] != 'S' || localMsg[2] != 'U' || localMsg[3] != 'C')
                    return;
                currIdx += 4;

                uint protocolVer = BitConverter.ToUInt16(localMsg, currIdx);
                currIdx += 2;

                if (protocolVer > MaxProtocolVersion)
                    return;

                uint packetSize = BitConverter.ToUInt16(localMsg, currIdx);
                currIdx += 2;

                if (packetSize < 0)
                    return;

                packetSize += 16; //size of header
                if (packetSize > localMsg.Length)
                {
                    return;
                }

                if (packetSize < localMsg.Length)
                {
                    var newMsg = new byte[packetSize];
                    Array.Copy(localMsg, newMsg, packetSize);
                    localMsg = newMsg;
                }

                var crcValue = BitConverter.ToUInt32(localMsg, currIdx);
                //zero out the crc32 in the packet once we got it since that's whats needed for calculation
                localMsg[currIdx++] = 0;
                localMsg[currIdx++] = 0;
                localMsg[currIdx++] = 0;
                localMsg[currIdx++] = 0;

                var crcCalc = Crc32Algorithm.Compute(localMsg);
                if (crcValue != crcCalc)
                    return;

                var clientId = BitConverter.ToUInt32(localMsg, currIdx);
                currIdx += 4;

                var messageType = BitConverter.ToUInt32(localMsg, currIdx);
                currIdx += 4;

                if (messageType == (uint)MessageType.DSUC_VersionReq)
                {
                    var outputData = new byte[8];
                    var outIdx = 0;
                    Array.Copy(BitConverter.GetBytes((uint)MessageType.DSUS_VersionRsp), 0, outputData, outIdx, 4);
                    outIdx += 4;
                    Array.Copy(BitConverter.GetBytes(MaxProtocolVersion), 0, outputData, outIdx, 2);
                    outIdx += 2;
                    outputData[outIdx++] = 0;
                    outputData[outIdx++] = 0;

                    SendPacket(clientEP, outputData);
                }
                else if (messageType == (uint)MessageType.DSUC_ListPorts)
                {
                    var numPadRequests = BitConverter.ToInt32(localMsg, currIdx);
                    currIdx += 4;
                    if (numPadRequests < 0 || numPadRequests > NUMBER_SLOTS)
                        return;

                    var requestsIdx = currIdx;
                    for (var i = 0; i < numPadRequests; i++)
                    {
                        var currRequest = localMsg[requestsIdx + i];
                        if (currRequest >= NUMBER_SLOTS)
                            return;
                    }

                    var outputData = new byte[16];
                    for (byte i = 0; i < numPadRequests; i++)
                    {
                        var currRequest = localMsg[requestsIdx + i];
                        var padData = new DualShockPadMeta();
                        portInfoGet(currRequest, ref padData);

                        var outIdx = 0;
                        Array.Copy(BitConverter.GetBytes((uint)MessageType.DSUS_PortInfo), 0, outputData, outIdx, 4);
                        outIdx += 4;

                        outputData[outIdx++] = padData.PadId;
                        outputData[outIdx++] = (byte)padData.PadState;
                        outputData[outIdx++] = (byte)padData.Model;
                        outputData[outIdx++] = (byte)padData.ConnectionType;

                        byte[] addressBytes = null;
                        if (padData.PadMacAddress != null)
                            addressBytes = padData.PadMacAddress.GetAddressBytes();

                        if (addressBytes != null && addressBytes.Length == 6)
                        {
                            outputData[outIdx++] = addressBytes[0];
                            outputData[outIdx++] = addressBytes[1];
                            outputData[outIdx++] = addressBytes[2];
                            outputData[outIdx++] = addressBytes[3];
                            outputData[outIdx++] = addressBytes[4];
                            outputData[outIdx++] = addressBytes[5];
                        }
                        else
                        {
                            outputData[outIdx++] = 0;
                            outputData[outIdx++] = 0;
                            outputData[outIdx++] = 0;
                            outputData[outIdx++] = 0;
                            outputData[outIdx++] = 0;
                            outputData[outIdx++] = 0;
                        }

                        outputData[outIdx++] = (byte)padData.BatteryStatus;
                        outputData[outIdx++] = 0;

                        SendPacket(clientEP, outputData);
                    }
                }
                else if (messageType == (uint)MessageType.DSUC_PadDataReq)
                {
                    var regFlags = localMsg[currIdx++];
                    var idToReg = localMsg[currIdx++];
                    PhysicalAddress macToReg = null;
                    {
                        var macBytes = new byte[6];
                        Array.Copy(localMsg, currIdx, macBytes, 0, macBytes.Length);
                        currIdx += macBytes.Length;
                        macToReg = new PhysicalAddress(macBytes);
                    }

                    lock (clients)
                    {
                        if (clients.ContainsKey(clientEP))
                        {
                            clients[clientEP].RequestPadInfo(regFlags, idToReg, macToReg);
                        }
                        else
                        {
                            var clientTimes = new ClientRequestTimes();
                            clientTimes.RequestPadInfo(regFlags, idToReg, macToReg);
                            clients[clientEP] = clientTimes;
                        }
                    }
                }
            }
            catch (Exception /*e*/)
            {
            }
        }

        private void ReceiveCallback(IAsyncResult iar)
        {
            byte[] localMsg = null;
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                //Get the received message.
                var recvSock = (Socket)iar.AsyncState;
                var msgLen = recvSock.EndReceiveFrom(iar, ref clientEP);

                localMsg = new byte[msgLen];
                Array.Copy(recvBuffer, localMsg, msgLen);
            }
            catch (Exception /*e*/)
            {
            }

            //Start another receive as soon as we copied the data
            StartReceive();

            //Process the data if its valid
            if (localMsg != null)
                ProcessIncoming(localMsg, (IPEndPoint)clientEP);
        }

        private void StartReceive()
        {
            try
            {
                if (running)
                {
                    //Start listening for a new message.
                    EndPoint newClientEP = new IPEndPoint(IPAddress.Any, 0);
                    udpSock.BeginReceiveFrom(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, ref newClientEP,
                        ReceiveCallback, udpSock);
                }
            }
            catch (SocketException /*ex*/)
            {
                var IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                var SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                udpSock.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);

                StartReceive();
            }
        }

        public void Start(int port, string listenAddress = "")
        {
            if (running)
            {
                if (udpSock != null)
                {
                    udpSock.Close();
                    udpSock = null;
                }

                running = false;
            }

            udpSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                IPAddress udpListenIPAddress;
                if (listenAddress == "127.0.0.1" || listenAddress == "")
                {
                    // Listen on local looback interface (default option). Does not allow remote client connections
                    udpListenIPAddress = IPAddress.Loopback;
                }
                else if (listenAddress == "0.0.0.0")
                {
                    // Listen on all IPV4 interfaces. 
                    // Remote client connections allowed. If the local network is not "safe" then may not be a good idea, because at the moment incoming connections are not authenticated in any way
                    udpListenIPAddress = IPAddress.Any;
                }
                else
                {
                    // Listen on a specific hostname or IPV4 interface address. If the hostname has multiple interfaces then use the first IPV4 address because it is usually the primary IP addr.
                    // Remote client connections allowed.
                    var ipAddresses = Dns.GetHostAddresses(listenAddress);
                    udpListenIPAddress = null;
                    foreach (var ip4 in ipAddresses.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
                    {
                        udpListenIPAddress = ip4;
                        break;
                    }

                    if (udpListenIPAddress == null) throw new SocketException(10049 /*WSAEADDRNOTAVAIL*/);
                }

                udpSock.Bind(new IPEndPoint(udpListenIPAddress, port));
            }
            catch (SocketException ex)
            {
                udpSock.Close();
                udpSock = null;

                throw ex;
            }

            var randomBuf = new byte[4];
            new Random().NextBytes(randomBuf);
            serverId = BitConverter.ToUInt32(randomBuf, 0);

            running = true;
            StartReceive();
        }

        public void Stop()
        {
            running = false;
            if (udpSock != null)
            {
                udpSock.Close();
                udpSock = null;
            }
        }

        private bool ReportToBuffer(DS4State hidReport, byte[] outputData, ref int outIdx)
        {
            unchecked
            {
                outputData[outIdx] = 0;

                if (hidReport.DpadLeft) outputData[outIdx] |= 0x80;
                if (hidReport.DpadDown) outputData[outIdx] |= 0x40;
                if (hidReport.DpadRight) outputData[outIdx] |= 0x20;
                if (hidReport.DpadUp) outputData[outIdx] |= 0x10;

                if (hidReport.Options) outputData[outIdx] |= 0x08;
                if (hidReport.R3) outputData[outIdx] |= 0x04;
                if (hidReport.L3) outputData[outIdx] |= 0x02;
                if (hidReport.Share) outputData[outIdx] |= 0x01;

                outputData[++outIdx] = 0;

                if (hidReport.Square) outputData[outIdx] |= 0x80;
                if (hidReport.Cross) outputData[outIdx] |= 0x40;
                if (hidReport.Circle) outputData[outIdx] |= 0x20;
                if (hidReport.Triangle) outputData[outIdx] |= 0x10;

                if (hidReport.R1) outputData[outIdx] |= 0x08;
                if (hidReport.L1) outputData[outIdx] |= 0x04;
                if (hidReport.R2Btn) outputData[outIdx] |= 0x02;
                if (hidReport.L2Btn) outputData[outIdx] |= 0x01;

                outputData[++outIdx] = hidReport.PS ? (byte)1 : (byte)0;
                outputData[++outIdx] = hidReport.TouchButton ? (byte)1 : (byte)0;

                //Left stick
                outputData[++outIdx] = hidReport.LX;
                outputData[++outIdx] = hidReport.LY;
                outputData[outIdx] = (byte)(255 - outputData[outIdx]); //invert Y by convention

                //Right stick
                outputData[++outIdx] = hidReport.RX;
                outputData[++outIdx] = hidReport.RY;
                outputData[outIdx] = (byte)(255 - outputData[outIdx]); //invert Y by convention

                //we don't have analog buttons on DS4 :(
                outputData[++outIdx] = hidReport.DpadLeft ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.DpadDown ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.DpadRight ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.DpadUp ? (byte)0xFF : (byte)0x00;

                outputData[++outIdx] = hidReport.Square ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.Cross ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.Circle ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.Triangle ? (byte)0xFF : (byte)0x00;

                outputData[++outIdx] = hidReport.R1 ? (byte)0xFF : (byte)0x00;
                outputData[++outIdx] = hidReport.L1 ? (byte)0xFF : (byte)0x00;

                outputData[++outIdx] = hidReport.R2;
                outputData[++outIdx] = hidReport.L2;

                outIdx++;

                //DS4 only: touchpad points
                for (var i = 0; i < 2; i++)
                {
                    var tpad = i == 0 ? hidReport.TrackPadTouch0 : hidReport.TrackPadTouch1;

                    outputData[outIdx++] = tpad.IsActive ? (byte)1 : (byte)0;
                    outputData[outIdx++] = tpad.Id;
                    Array.Copy(BitConverter.GetBytes((ushort)tpad.X), 0, outputData, outIdx, 2);
                    outIdx += 2;
                    Array.Copy(BitConverter.GetBytes((ushort)tpad.Y), 0, outputData, outIdx, 2);
                    outIdx += 2;
                }

                //motion timestamp
                if (hidReport.Motion != null)
                    Array.Copy(BitConverter.GetBytes(hidReport.totalMicroSec), 0, outputData, outIdx, 8);
                else
                    Array.Clear(outputData, outIdx, 8);

                outIdx += 8;

                //accelerometer
                if (hidReport.Motion != null)
                {
                    Array.Copy(BitConverter.GetBytes((float)hidReport.Motion.accelXG), 0, outputData, outIdx, 4);
                    outIdx += 4;
                    Array.Copy(BitConverter.GetBytes((float)hidReport.Motion.accelYG), 0, outputData, outIdx, 4);
                    outIdx += 4;
                    Array.Copy(BitConverter.GetBytes((float)-hidReport.Motion.accelZG), 0, outputData, outIdx, 4);
                    outIdx += 4;
                }
                else
                {
                    Array.Clear(outputData, outIdx, 12);
                    outIdx += 12;
                }

                //gyroscope
                if (hidReport.Motion != null)
                {
                    Array.Copy(BitConverter.GetBytes((float)hidReport.Motion.angVelPitch), 0, outputData, outIdx, 4);
                    outIdx += 4;
                    Array.Copy(BitConverter.GetBytes((float)hidReport.Motion.angVelYaw), 0, outputData, outIdx, 4);
                    outIdx += 4;
                    Array.Copy(BitConverter.GetBytes((float)hidReport.Motion.angVelRoll), 0, outputData, outIdx, 4);
                    outIdx += 4;
                }
                else
                {
                    Array.Clear(outputData, outIdx, 12);
                    outIdx += 12;
                }
            }

            return true;
        }

        public void NewReportIncoming(ref DualShockPadMeta padMeta, DS4State hidReport, byte[] outputData)
        {
            if (!running)
                return;

            var clientsList = new List<IPEndPoint>();
            var now = DateTime.UtcNow;
            lock (clients)
            {
                var clientsToDelete = new List<IPEndPoint>();

                foreach (var cl in clients)
                {
                    const double TimeoutLimit = 5;

                    if ((now - cl.Value.AllPadsTime).TotalSeconds < TimeoutLimit)
                    {
                        clientsList.Add(cl.Key);
                    }
                    else if (padMeta.PadId < cl.Value.PadIdsTime.Length &&
                             (now - cl.Value.PadIdsTime[padMeta.PadId]).TotalSeconds < TimeoutLimit)
                    {
                        clientsList.Add(cl.Key);
                    }
                    else if (cl.Value.PadMacsTime.ContainsKey(padMeta.PadMacAddress) &&
                             (now - cl.Value.PadMacsTime[padMeta.PadMacAddress]).TotalSeconds < TimeoutLimit)
                    {
                        clientsList.Add(cl.Key);
                    }
                    else //check if this client is totally dead, and remove it if so
                    {
                        var clientOk = false;
                        for (var i = 0; i < cl.Value.PadIdsTime.Length; i++)
                        {
                            var dur = (now - cl.Value.PadIdsTime[i]).TotalSeconds;
                            if (dur < TimeoutLimit)
                            {
                                clientOk = true;
                                break;
                            }
                        }

                        if (!clientOk)
                        {
                            foreach (var dict in cl.Value.PadMacsTime)
                            {
                                var dur = (now - dict.Value).TotalSeconds;
                                if (dur < TimeoutLimit)
                                {
                                    clientOk = true;
                                    break;
                                }
                            }

                            if (!clientOk)
                                clientsToDelete.Add(cl.Key);
                        }
                    }
                }

                foreach (var delCl in clientsToDelete) clients.Remove(delCl);
                clientsToDelete.Clear();
                clientsToDelete = null;
            }

            if (clientsList.Count <= 0)
                return;

            unchecked
            {
                //byte[] outputData = new byte[100];
                var outIdx = BeginPacket(outputData);
                Array.Copy(BitConverter.GetBytes((uint)MessageType.DSUS_PadDataRsp), 0, outputData, outIdx, 4);
                outIdx += 4;

                outputData[outIdx++] = padMeta.PadId;
                outputData[outIdx++] = (byte)padMeta.PadState;
                outputData[outIdx++] = (byte)padMeta.Model;
                outputData[outIdx++] = (byte)padMeta.ConnectionType;
                {
                    var padMac = padMeta.PadMacAddress.GetAddressBytes();
                    outputData[outIdx++] = padMac[0];
                    outputData[outIdx++] = padMac[1];
                    outputData[outIdx++] = padMac[2];
                    outputData[outIdx++] = padMac[3];
                    outputData[outIdx++] = padMac[4];
                    outputData[outIdx++] = padMac[5];
                }
                outputData[outIdx++] = (byte)padMeta.BatteryStatus;
                outputData[outIdx++] = padMeta.IsActive ? (byte)1 : (byte)0;

                Array.Copy(BitConverter.GetBytes(hidReport.PacketCounter), 0, outputData, outIdx, 4);
                outIdx += 4;

                if (!ReportToBuffer(hidReport, outputData, ref outIdx))
                    return;
                FinishPacket(outputData);

                foreach (var cl in clientsList)
                {
                    //try { udpSock.SendTo(outputData, cl); }
                    var temp = 0;
                    poolLock.EnterWriteLock();
                    temp = listInd;
                    listInd = ++listInd % ARG_BUFFER_LEN;
                    var args = argsList[temp];
                    poolLock.ExitWriteLock();

                    _pool.Wait();
                    args.RemoteEndPoint = cl;
                    Array.Copy(outputData, args.Buffer, outputData.Length);
                    var sentAsync = false;
                    try
                    {
                        sentAsync = udpSock.SendToAsync(args);
                    }
                    catch (SocketException /*ex*/)
                    {
                    }
                    finally
                    {
                        if (!sentAsync) CompletedSynchronousSocketEvent();
                    }
                }
            }

            clientsList.Clear();
            clientsList = null;
        }

        private enum MessageType
        {
            DSUC_VersionReq = 0x100000,
            DSUS_VersionRsp = 0x100000,
            DSUC_ListPorts = 0x100001,
            DSUS_PortInfo = 0x100001,
            DSUC_PadDataReq = 0x100002,
            DSUS_PadDataRsp = 0x100002
        }

        private class ClientRequestTimes
        {
            public ClientRequestTimes()
            {
                AllPadsTime = DateTime.MinValue;
                PadIdsTime = new DateTime[4];

                for (var i = 0; i < PadIdsTime.Length; i++)
                    PadIdsTime[i] = DateTime.MinValue;

                PadMacsTime = new Dictionary<PhysicalAddress, DateTime>();
            }

            public DateTime AllPadsTime { get; private set; }

            public DateTime[] PadIdsTime { get; }

            public Dictionary<PhysicalAddress, DateTime> PadMacsTime { get; }

            public void RequestPadInfo(byte regFlags, byte idToReg, PhysicalAddress macToReg)
            {
                if (regFlags == 0)
                {
                    AllPadsTime = DateTime.UtcNow;
                }
                else
                {
                    if ((regFlags & 0x01) != 0) //id valid
                        if (idToReg < PadIdsTime.Length)
                            PadIdsTime[idToReg] = DateTime.UtcNow;
                    if ((regFlags & 0x02) != 0) //mac valid
                        PadMacsTime[macToReg] = DateTime.UtcNow;
                }
            }
        }
    }
}