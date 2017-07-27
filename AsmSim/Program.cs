using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmSim
{
    class Program
    {

        const string asmv = "v0.3";
        const string simv = "v0.45";

        static void Main(string[] args)
        {
            byte a, b = a = 0;
            byte[,] ram = new byte[256, 2];
            byte[] flags = new byte[8];

            Console.Title = "AsmSim " + simv + "";

            while (true)
            {
                Select(true);
                string str;
                Console.SetCursorPosition(0, 0);
                Console.Write((str = "    AsmSim " + simv + " with JAsm " + asmv + " | RISC Assembly simulation") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                Select(false);
                int c = Menu(new string[] { "Program", "Run", "Save", "Load", "Clear", "Info", "Quit" }, "SELECT ACTION:\n", "    JAsm " + asmv + " | RISC Assembly simulation");

                if (c == 0)
                {
                    c = 0;
                    Console.CursorVisible = false;
                    Console.Clear();
                    bool p = true;
                    int cc = 0;
                    ConsoleKeyInfo key;
                    while (p)
                    {
                        key = new ConsoleKeyInfo('a',ConsoleKey.A,true,true,true);
                        if (c < 128 && c >= 0)
                        {
                            Select(true);
                            Console.SetCursorPosition(0, 0);
                            Console.Write((str = "    PROGRAM MODE | Ctrl-Q: Exit | Enter: Next Addr | Up: Prev Add | Down: Next Addr | Backspace: Left") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write((str = "    128-addr | 8-bit | RISC JAsm " + asmv + "") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                            Console.SetCursorPosition(0, 0);
                            Console.SetCursorPosition(0, 1);
                            for (int i = 0; i < Console.WindowHeight - 2; i++)
                            {
                                if (c + i > 127)
                                {
                                    Console.Write(string.Concat(Enumerable.Repeat(" ", Console.WindowWidth)));
                                    continue;
                                }
                                char[] ch = ToHex(ram[c + i, 0]);
                                Select(false);
                                Console.Write("{0}> ", (c + i).ToString("000"));
                                Select(cc == 0 && i == 0);
                                Console.Write(ch[0]);
                                Select(cc == 1 && i == 0);
                                Console.Write(ch[1]);
                                ch = ToHex(ram[c + i, 1]);
                                Select(cc == 2 && i == 0);
                                Console.Write(ch[0]);
                                Select(cc == 3 && i == 0);
                                Console.Write(ch[1]);
                                Console.SetCursorPosition(0, Console.CursorTop + 1);
                            }
                            Console.SetCursorPosition(0, 0);
                            if ((key = Console.ReadKey(true)).Key == ConsoleKey.Q && key.Modifiers == ConsoleModifiers.Control)
                            {
                                break;
                            }
                            else if (key.Key == ConsoleKey.UpArrow)
                            {
                                c--;
                            }
                            else if (key.Key == ConsoleKey.DownArrow)
                            {
                                c++;
                            }
                            else if (key.Key == ConsoleKey.Enter)
                            {
                                c++;
                                cc = 0;
                            }
                            else if (key.Key == ConsoleKey.Backspace)
                            {
                                if (cc > 0) cc--;
                            }
                            else
                            {
                                byte i = ParseKey(key.Key);
                                if (i != 16)
                                {
                                    char[] arr = ToHex(ram[c, cc > 1 ? 1 : 0]);
                                    arr[cc == 0 || cc == 2 ? 0 : 1] = ToHex(i)[1];
                                    ram[c, cc > 1 ? 1 : 0] = ToByte(arr);
                                    if (cc < 3) cc++;
                                }
                            }

                            if (c > 127) c = 0;
                            else if (c < 0) c = 127;
                        }
                    }
                }
                else if (c == 1)
                {
                    Console.Clear();

                    for (int i = 0; i < 128; i++)
                    {
                        byte com = ram[i, 0];
                        byte val = ram[i, 1];

                        //HLT - Halt
                        if (com == 0)
                        {
                            Console.Write("HLT\n");
                            Console.WriteLine("Press any key to exit.");
                            Console.ReadKey();
                            break;
                        }
                        //STA - Store A - 01
                        else if (com == 1)
                        {
                            a = val;
                        }
                        //MVB - Move B - 02
                        else if (com == 2)
                        {
                            b = a;
                        }
                        //MVA - Move A - 03
                        else if (com == 3)
                        {
                            a = b;
                        }
                        //MVR - Move Ram - 04
                        else if (com == 4)
                        {
                            ram[val, flags[1]] = a;
                        }
                        //MRA - Move Ram A - 05
                        else if (com == 5)
                        {
                            a = ram[val, flags[1]];
                        }
                        //SRF - Set Ram Flag (Ram flag determines which byte of ram to use) - 06
                        else if (com == 6)
                        {
                            if (val == 0 || val == 1)
                            {
                                flags[1] = val;
                            }
                        }
                        //CMP - Compare A and B - 07
                        else if (com == 7)
                        {
                            flags[0] = 2;
                            if (a > b)
                            {
                                flags[0] = 0;
                            }
                            if (a == b)
                            {
                                flags[0] = 1;
                            }
                        }
                        //JL - Jump Less - 08
                        else if (com == 8)
                        {
                            if (flags[0] == 2)
                            {
                                i = val;
                            }
                        }
                        //JEQ - Jump Equal - 09
                        else if (com == 9)
                        {
                            if (flags[0] == 1)
                            {
                                i = val;
                            }
                        }
                        //JG - Jump Greater - 0A
                        else if (com == 10)
                        {
                            if (flags[0] == 0)
                            {
                                i = val;
                            }
                        }
                        //JMP - Jump - 0B
                        else if (com == 11)
                        {
                            i = val;
                        }
                        //ADD - Add A and B - 0C
                        else if (com == 12)
                        {
                            a += b;
                        }
                        //SUB - Subtract A and B - 0D
                        else if (com == 13)
                        {
                            a -= b;
                        }
                        //MUL - Multiply A and B - 0E
                        else if (com == 14)
                        {
                            a *= b;
                        }
                        //DIV - Divide A and B - 0F
                        else if (com == 15)
                        {
                            a /= b;
                        }
                        //OUT - Output A as a number - 10
                        else if (com == 16)
                        {
                            Console.Write(a);
                        }
                        //OUC - Output A as a character - 11
                        else if (com == 17)
                        {
                            Console.Write((char)a);
                        }
                        //OUB - Output A as a Hexadecimal Byte - 12
                        else if (com == 18)
                        {
                            Console.Write(ToHex(a));
                        }
                        //GCH - (getch) Get character from prompt - 13
                        else if (com == 19)
                        {
                            a = (byte)Console.Read();
                        }
                        //GNM - Get Number from prompt - 14
                        else if (com == 20)
                        {
                            a = CharNum((byte)Console.Read());
                        }
                        //GET - Get Byte from prompt - 15
                        else if (com == 20)
                        {
                            char o1 = Console.ReadKey().KeyChar;
                            char o2 = Console.ReadKey().KeyChar;
                            a = ToByte(new char[] { o1, o2 });
                        }

                    }
                }
                else if (c == 2)
                {
                    string pwd = "";
                    bool p = true;
                    ConsoleKeyInfo ckey;
                    while (p)
                    {
                        pwd = "";
                        Console.Clear();
                        Select(true);
                        Console.SetCursorPosition(0, 0);
                        Console.Write((str = "    SAVE PROGRAM | JAsm " + asmv + "") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                        Console.Write((str = "    JAsm " + asmv + " | Ctrl+Q to return") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                        Console.SetCursorPosition(0, 0);
                        Console.SetCursorPosition(0, 2);
                        Console.Write("File Path:");
                        Select(false);
                        Console.Write(" ");
                        Console.CursorVisible = true;
                        while (true)
                        {
                            ckey = Console.ReadKey();
                            if (ckey.Key == ConsoleKey.Enter)
                            {
                                break;
                            }
                            else if (ckey.Key == ConsoleKey.Q)
                            {
                                p = false;
                                break;
                            }
                            else
                            {
                                pwd += (char)ckey.KeyChar;
                            }
                        }
                        if (p == false) break;
                        pwd = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + pwd;
                        if (File.Exists(pwd))
                        {
                            Select(true);
                            Console.CursorVisible = false;
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write((str = "    FILENAME TAKEN | OVERRIDE? [Y/N]") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                            Console.SetCursorPosition(0, 0);
                            ckey = Console.ReadKey();
                            while (ckey.Key != ConsoleKey.Y || ckey.Key != ConsoleKey.Y) ;
                            if (ckey.Key == ConsoleKey.N) continue;
                        }
                        File.Delete(pwd);
                        Bitmap bmp = new Bitmap(16, 8);
                        for (int i = 0; i < 16; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                bmp.SetPixel(i, j, Color.FromArgb(ram[i * 8 + j, 0], ram[i * 8 + j, 1], 127));
                            }
                        }
                        bmp.Save(pwd);
                        Select(true);
                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                        Console.Write((str = "    JAsm " + asmv + " | Ctrl+Q to return | Any key to continue | FILE SAVED") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                        Console.SetCursorPosition(0, 0);
                        Select(false);
                        if (Console.ReadKey().Key == ConsoleKey.Q) break;
                    }
                }
                else if (c == 3)
                {
                    string pwd = "";
                    bool p = true;
                    ConsoleKeyInfo ckey;
                    Console.Clear();
                    Select(true);
                    Console.SetCursorPosition(0, Console.WindowHeight - 1);
                    Console.Write((str = "    JAsm " + asmv + " | Ctrl+Q to return") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                    while (p)
                    {
                        Select(false);
                        pwd = "";
                        Select(true);
                        Console.SetCursorPosition(0, 0);
                        Console.Write((str = "    LOAD PROGRAM | JAsm " + asmv + "") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                        Console.SetCursorPosition(0, 0);
                        Console.SetCursorPosition(0, 2);
                        Console.Write(str = "File Path:");
                        Select(false);
                        Console.Write(string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                        Console.SetCursorPosition(str.Length + 1, 2);
                        Console.CursorVisible = true;
                        while (true)
                        {
                            ckey = Console.ReadKey();
                            if (ckey.Key == ConsoleKey.Enter)
                            {
                                break;
                            }
                            else if (ckey.Key == ConsoleKey.Q && ckey.Modifiers == ConsoleModifiers.Control)
                            {
                                p = false;
                                break;
                            }
                            else
                            {
                                pwd += ckey.KeyChar;
                            }
                        }
                        if (!p) break;
                        pwd = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + pwd;
                        if (File.Exists(pwd))
                        {
                            Bitmap bmp = new Bitmap(pwd);
                            for (int i = 0; i < 16; i++)
                            {
                                for (int j = 0; j < 8; j++)
                                {
                                    ram[i * 8 + j, 0] = bmp.GetPixel(i, j).R;
                                    ram[i * 8 + j, 1] = bmp.GetPixel(i, j).G;
                                }
                            }
                            Select(true);
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write((str = "    JAsm " + asmv + " | Ctrl+Q to return | PROGRAM LOADED") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                            Console.SetCursorPosition(0, 0);
                            bmp.Dispose();
                        }
                        else
                        {
                            Select(true);
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write((str = "    JAsm " + asmv + " | Ctrl+Q to return | FILE NOT FOUND") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                            Console.SetCursorPosition(0, 0);
                        }
                    }
                }
                else if (c == 4)
                {
                    Select(true);
                    Console.SetCursorPosition(0, Console.WindowHeight - 1);
                    Console.Write((str = "    JAsm " + asmv + " | Clear program memory? [Y/N]") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                    Console.SetCursorPosition(0, 0);
                    if (Console.ReadKey().Key == ConsoleKey.Y)
                    {
                        ram = new byte[256, 2];
                        a = b = 0;
                    }
                    Select(false);
                }
                else if (c == 5)
                {
                    while (true)
                    {
                        c = Menu(new string[] { "JAsm", "AsmSim", "Back" }, "SELECT ACTION:\n", "AsmSim " + simv + " with JAsm " + asmv + " | Info");
                        if (c == 0)
                        {
                            int scrl = 0;
                            ConsoleKeyInfo ckey;
                            string[] txt = new string[]
                            {
                            "JAsm " + asmv + " by JuhaJGamer 2017",
                            "Command refrence",
                            " Command construction: 00 00",
                            "                       || ||",
                            "         Instruction byte ||",
                            "          Argument(val) byte",
                            " Commands:                  ",
                            "  00 - HLT - Halts the program",
                            "  01 - STA - Stores a value into register A",
                            "  02 - MVB - Moves a value from A to B",
                            "  03 - MVA - Moves a value from B to A",
                            "  04 - MVR - Moves a value from A to the specified RAM address",
                            "   Note: The flag set by SFR sets which byte of RAM the value is stored in [0,1]",
                            "  05 - MRA - \"Move RAM A\", Moves a value from the specified RAM address to A",
                            "   Note: The note on instruction 04 also applies here. Use SFR to set flag",
                            "  06 - SFR - Sets the \"RAM\" flag, value specifies whether to use the First byte (0) or the second byte (1)",
                            "  07 - CMP - Compares A and B and sets a flag specified by the result",
                            "  08 - JL  - Jump Less, Jumps to address specified by val if A is less that B",
                            "  09 - JEQ - Jump Equal, Jumps to address specified by val if A and B are equal",
                            "  0A - JG  - Jump Greater, Jumps to address specified by val if A is greater than B",
                            "   Note: Does not compare on command run, relies on result produced by the CMP command",
                            "  0B - JMP - Jumps to an address specified by val",
                            "  0C - ADD - Adds B to A and stores the result in A",
                            "  0D - SUB - Subtracts B from A and stores the result in A",
                            "  0E - MUL - Multiplies A by B and store sthe result in A",
                            "  0F - DIV - Divides A by B and stores the result in A",
                            "   Note: No decimals, truncated answer",
                            "  10 - OUT - Outputs A as a number (Ie. A = 10, OUT => \"10\")",
                            "  11 - OUC - Outputs A as a character (Ie. A=6C, OUC => \"l\")",
                            "  12 - OUB - Outputs A as a hexadecimal byte (Ie. A=1A =>\"1A\")",
                            "  13 - GCH - Getch, Gets a character from the prompt and stores it in A",
                            "  14 - GNM - Getnum, Gets a number from the prompt and stores it in A",
                            "  15 - GET - Gets a hexadecimal byte from the prompt and stores it in A",
                            "   Note: GET asks for 2 characters (to complete a byte), but GCH and GNM and for only 1",
                            };
                            while (true)
                            {
                                Console.Clear();
                                Select(true);
                                Console.SetCursorPosition(0, 0);
                                Console.Write((str = "    JAsm " + asmv + " | Info ") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                                Console.Write((str = "    JAsm " + asmv + " | Ctrl+Q to exit | Up: Scroll up| Down: Scroll down") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                                Console.SetCursorPosition(0, 0);
                                Console.SetCursorPosition(0, 1);
                                Select(false);
                                for (int i = 0; i < Console.WindowHeight - 3; i++)
                                {
                                    Console.WriteLine(txt[scrl + i]);
                                }
                                ckey = Console.ReadKey();
                                if (ckey.Key == ConsoleKey.UpArrow)
                                {
                                    if (scrl > 0) scrl--;
                                }
                                else if (ckey.Key == ConsoleKey.DownArrow)
                                {
                                    if (scrl < txt.Length - (Console.WindowHeight - 3)) scrl++;
                                }
                                else if (ckey.Key == ConsoleKey.Q && ckey.Modifiers == ConsoleModifiers.Control)
                                {
                                    break;
                                }
                            }
                        }
                        else if (c == 1)
                        {
                            int scrl = 0;
                            ConsoleKeyInfo ckey;
                            string[] txt = new string[]
                            {
                                "AsmSim "+simv+" by JuhaJGamer 2017",
                                "Replicates a JAsm architechture processor,",
                                "current JASm version "+asmv+"",
                                " ",
                                "Changelog:",
                                " Updated client to v0.45,\n made easily changeable version numbers,\n Changed program mode into using ctrl + q for exiting.",
                            };
                            while (true)
                            {
                                Console.Clear();
                                Select(true);
                                Console.SetCursorPosition(0, 0);
                                Console.Write((str = "    AsmSim " + simv + " | Info ") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                                Console.Write((str = "    AsmSim " + simv + " | Ctrl+Q to exit " + ((txt.Length > (Console.WindowHeight - 3)) ? "| Up: Scroll up| Down: Scroll down" : "")) + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                                Console.SetCursorPosition(0, 0);
                                Console.SetCursorPosition(0, 1);
                                Select(false);
                                for (int i = 0; i < ((txt.Length > Console.WindowHeight - 3) ? Console.WindowHeight - 3 : txt.Length); i++)
                                {
                                    Console.WriteLine(txt[scrl + i]);
                                }
                                ckey = Console.ReadKey();
                                if (ckey.Key == ConsoleKey.UpArrow)
                                {
                                    if (scrl > 0) scrl--;
                                }
                                else if (ckey.Key == ConsoleKey.DownArrow)
                                {
                                    if (scrl < txt.Length - (Console.WindowHeight - 3)) scrl++;
                                }
                                else if (ckey.Key == ConsoleKey.Q && ckey.Modifiers == ConsoleModifiers.Control)
                                {
                                    break;
                                }
                            }
                        }
                        else if (c == 2)
                        {
                            break;
                        }
                    }
                }
                else if (c == 6)
                {
                    Select(true);
                    Console.SetCursorPosition(0, Console.WindowHeight - 1);
                    Console.Write((str = "    JAsm " + asmv + " | Quit? [Y/N]") + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                    Console.SetCursorPosition(0, 0);
                    if (Console.ReadKey().Key == ConsoleKey.Y) break;
                    Select(false);
                }
            }
        }

        //Key parser
        private static byte ParseKey(ConsoleKey key)
        {
            if ((int)key == 48) return 0;
            else if ((int)key == 49) return 1;
            else if ((int)key == 50) return 2;
            else if ((int)key == 51) return 3;
            else if ((int)key == 52) return 4;
            else if ((int)key == 53) return 5;
            else if ((int)key == 54) return 6;
            else if ((int)key == 55) return 7;
            else if ((int)key == 56) return 8;
            else if ((int)key == 57) return 9;
            else if ((int)key == 65) return 10;
            else if ((int)key == 66) return 11;
            else if ((int)key == 67) return 12;
            else if ((int)key == 68) return 13;
            else if ((int)key == 69) return 14;
            else if ((int)key == 70) return 15;
            else return 16;
        }

        //Char to num
        private static byte CharNum(byte key)
        {
            int ret1 = 0;
            if (int.TryParse(new string(new char[] { (char)key }), out ret1))
            {
                return (byte)ret1;
            }
            return 0;
        }

        //Byte to hexadecimal
        public static char[] ToHex(byte v)
        {
            return BitConverter.ToString(new byte[] { v }).ToCharArray();
        }

        //Hex to byte
        public static byte ToByte(char[] v)
        {
            return (byte)int.Parse(new string(v), System.Globalization.NumberStyles.HexNumber);
        }

        //Selection fucntion, makes text look "selected";
        private static void Select(bool b)
        {
            if (b)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void dummy() { }

        private static int Menu(string[] v1, string v2, string v3)
        {
            //Might just throw that now because that saves... memory? or maybe cpu? anyways it does less work if you do like this.
            Console.CursorVisible = false;
            MenuItem[] items = new MenuItem[v1.Length];
            int active = 0;
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new MenuItem(i > 0 ? false : true, new Action(dummy), v1[i]);
            }
            while (true)
            {
                Console.Clear();
                Select(true);
                string str;
                Console.SetCursorPosition(0, 0);
                Console.Write((str = v3) + string.Concat(Enumerable.Repeat(" ", Console.WindowWidth - str.Length)));
                Console.SetCursorPosition(0, 1);
                Select(false);
                Console.Write(v2);
                foreach (MenuItem i in items)
                {
                    Console.BackgroundColor = i.active ? ConsoleColor.White : ConsoleColor.Black;
                    Console.ForegroundColor = i.active ? ConsoleColor.Black : ConsoleColor.White;
                    Console.WriteLine(i.text);
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleKey key = Console.ReadKey().Key;
                if (key == ConsoleKey.UpArrow)
                {
                    if (active > 0)
                    {
                        items[active].active = false;
                        items[active - 1].active = true;
                        active--;
                    }
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    if (active < items.Length - 1)
                    {
                        items[active].active = false;
                        items[active + 1].active = true;
                        active++;
                    }
                }
                else if (key == ConsoleKey.Enter)
                {
                    Console.CursorVisible = false;
                    return active;
                }
            }
        }

        public struct MenuItem
        {
            public bool active;
            public Action callback;
            public string text;

            public MenuItem(bool active, Action callback, string text)
            {
                this.active = active;
                this.callback = callback;
                this.text = text;
            }
        }
    }
}
