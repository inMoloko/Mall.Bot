using System.Text.RegularExpressions;

namespace Mall.Bot.Common.MallHelpers.Models
{
    public class CodeModel
    {
        public int TerminalID { get; set; }
        public string Synonym { get; set; }
        public int MabObjectID { get; set; }
        public bool IsError = false;
        public CodeModel(string code)
        {
            code = Regex.Replace(code, @"\s+", "");
            int i = 0;
            string terminalID = "";
            while (i < code.Length && char.IsNumber(code[i]))
            {
                terminalID += code[i];
                i++;
            }

            while (i < code.Length && !char.IsNumber(code[i]))
            {
                Synonym += code[i];
                i++;
            }

            string mabObjectID = "";
            while (i < code.Length && char.IsNumber(code[i]))
            {
                mabObjectID += code[i];
                i++;
            }
            int temp;
            if (int.TryParse(terminalID, out temp))
            {
                TerminalID = temp;
                if (int.TryParse(mabObjectID, out temp))
                {
                    MabObjectID = temp;
                }
                else IsError = true;
            }
            else IsError = true;

            if (string.IsNullOrWhiteSpace(Synonym)) IsError = true;

        }
    }
}
