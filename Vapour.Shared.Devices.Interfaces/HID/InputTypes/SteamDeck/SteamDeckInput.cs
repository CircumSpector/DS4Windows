using System.Runtime.InteropServices;

namespace Vapour.Shared.Devices.HID.InputTypes.SteamDeck;
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SteamDeckInput
{
    public byte repid;
    public byte ptype;          
    public byte _a1;            
    public byte _a2;            
    public byte _a3;            
    public UInt32 seq;          
    public SteamDeckButtons0 buttons0;     
    public byte buttons1;       
    public byte buttons2;       
    public byte buttons3;       
    public byte buttons4;       
    public byte buttons5;       
    public Int16 lpad_x;        
    public Int16 lpad_y;        
    public Int16 rpad_x;        
    public Int16 rpad_y;        
    public Int16 accel_x;       
    public Int16 accel_y;       
    public Int16 accel_z;       
    public Int16 gpitch;        
    public Int16 gyaw;          
    public Int16 groll;         
    public Int16 q1;            
    public Int16 q2;            
    public Int16 q3;            
    public Int16 q4;            
    public Int16 ltrig;         
    public Int16 rtrig;         
    public Int16 lthumb_x;      
    public Int16 lthumb_y;      
    public Int16 rthumb_x;      
    public Int16 rthumb_y;      
    public Int16 lpad_pressure; 
    public Int16 rpad_pressure; 
}

