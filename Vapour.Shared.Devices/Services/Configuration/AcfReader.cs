using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Vapour.Shared.Devices.Services.Configuration;

/// <summary>
///     Steam app manifest reader.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class AcfReader
{
    public AcfReader(string fileLocation)
    {
        if (File.Exists(fileLocation))
        {
            FileLocation = fileLocation;
        }
        else
        {
            throw new FileNotFoundException("Error", fileLocation);
        }
    }

    public string FileLocation { get; }

    public bool CheckIntegrity()
    {
        string content = File.ReadAllText(FileLocation);
        int quote = content.Count(x => x == '"');
        int braceleft = content.Count(x => x == '{');
        int braceright = content.Count(x => x == '}');

        return braceleft == braceright && quote % 2 == 0;
    }

    public AcfStruct AcfFileToStruct()
    {
        return AcfFileToStruct(File.ReadAllText(FileLocation));
    }

    private static AcfStruct AcfFileToStruct(string regionToReadIn)
    {
        AcfStruct acf = new();
        int lengthOfRegion = regionToReadIn.Length;
        int currentPos = 0;
        while (lengthOfRegion > currentPos)
        {
            int firstItemStart = regionToReadIn.IndexOf('"', currentPos);
            if (firstItemStart == -1)
            {
                break;
            }

            int firstItemEnd = regionToReadIn.IndexOf('"', firstItemStart + 1);
            currentPos = firstItemEnd + 1;
            string firstItem = regionToReadIn.Substring(firstItemStart + 1, firstItemEnd - firstItemStart - 1);

            int secondItemStartQuote = regionToReadIn.IndexOf('"', currentPos);
            int secondItemStartBraceleft = regionToReadIn.IndexOf('{', currentPos);
            if (secondItemStartBraceleft == -1 || secondItemStartQuote < secondItemStartBraceleft)
            {
                int secondItemEndQuote = regionToReadIn.IndexOf('"', secondItemStartQuote + 1);
                string secondItem = regionToReadIn.Substring(secondItemStartQuote + 1,
                    secondItemEndQuote - secondItemStartQuote - 1);
                currentPos = secondItemEndQuote + 1;
                acf.SubItems.Add(firstItem, secondItem);
            }
            else
            {
                int secondItemEndBraceright = regionToReadIn.NextEndOf('{', '}', secondItemStartBraceleft + 1);
                AcfStruct acfs = AcfFileToStruct(regionToReadIn.Substring(secondItemStartBraceleft + 1,
                    secondItemEndBraceright - secondItemStartBraceleft - 1));
                currentPos = secondItemEndBraceright + 1;
                acf.SubAcf.Add(firstItem, acfs);
            }
        }

        return acf;
    }
}

/// <summary>
///     Steam app manifest entry.
/// </summary>
public class AcfStruct
{
    public AcfStruct()
    {
        SubAcf = new Dictionary<string, AcfStruct>();
        SubItems = new Dictionary<string, string>();
    }

    public Dictionary<string, AcfStruct> SubAcf { get; }
    public Dictionary<string, string> SubItems { get; }

    public override string ToString()
    {
        return ToString(0);
    }

    private string ToString(int depth)
    {
        StringBuilder sb = new();
        foreach (KeyValuePair<string, string> item in SubItems)
        {
            sb.Append('\t', depth);
            sb.Append($"\"{item.Key}\"\t\t\"{item.Value}\"\r\n");
        }

        foreach (KeyValuePair<string, AcfStruct> item in SubAcf)
        {
            sb.Append('\t', depth);
            sb.Append($"\"{item.Key}\"\n");
            sb.Append('\t', depth);
            sb.AppendLine("{");
            sb.Append(item.Value.ToString(depth + 1));
            sb.Append('\t', depth);
            sb.AppendLine("}");
        }

        return sb.ToString();
    }
}

internal static class Extension
{
    public static int NextEndOf(this string str, char open, char close, int startIndex)
    {
        if (open == close)
        {
            throw new Exception("\"Open\" and \"Close\" char are equivalent!");
        }

        int openItem = 0;
        int closeItem = 0;
        for (int i = startIndex; i < str.Length; i++)
        {
            if (str[i] == open)
            {
                openItem++;
            }

            if (str[i] == close)
            {
                closeItem++;
                if (closeItem > openItem)
                {
                    return i;
                }
            }
        }

        throw new Exception("Not enough closing characters!");
    }
}
