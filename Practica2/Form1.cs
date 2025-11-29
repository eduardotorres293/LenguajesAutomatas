using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Practica2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            analizarToolStripMenuItem.Enabled = false;
        }

        private List<string> P_Reservadas = new List<string>()
        {
            "auto", "break", "case", "char", "const", "continue", "default", "do",
            "double", "else", "enum", "extern", "float", "for", "goto", "if", "include",
            "inline", "int", "long", "main", "register", "restrict", "return", "short",
            "signed", "sizeof", "static", "struct", "switch", "typedef", "union",
            "unsigned", "void", "volatile", "while", "_Alignas", "_Alignof",
            "_Atomic", "_Bool", "_Complex", "_Generic", "_Imaginary", "_Noreturn",
            "_Static_assert", "_Thread_local"
        };

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog VentanaAbrir = new OpenFileDialog();
            VentanaAbrir.Filter = "Texto|*.c";
            if (VentanaAbrir.ShowDialog() == DialogResult.OK)
            {
                archivo = VentanaAbrir.FileName;
                using (StreamReader LeerF = new StreamReader(archivo))
                {
                    richTextBox1.Text = LeerF.ReadToEnd();
                }
            }
            this.Text = "Mi Compilador - " + archivo;
            analizarToolStripMenuItem.Enabled = true;
        }

        private void guardar()
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog();
            VentanaGuardar.Filter = "Texto|*.c";
            if (archivo != null)
            {
                using (StreamWriter EscribirF = new StreamWriter(archivo))
                {
                    EscribirF.Write(richTextBox1.Text);
                }
            }
            else
            {
                if (VentanaGuardar.ShowDialog() == DialogResult.OK)
                {
                    archivo = VentanaGuardar.FileName;
                    using (StreamWriter EscribirF = new StreamWriter(archivo))
                    {
                        EscribirF.Write(richTextBox1.Text);
                    }
                }
            }
            this.Text = "Mi Compilador - " + archivo;
        }

        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guardar();
        }

        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            archivo = null;
        }

        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog();
            VentanaGuardar.Filter = "Texto|*.c";
            if (VentanaGuardar.ShowDialog() == DialogResult.OK)
            {
                archivo = VentanaGuardar.FileName;
                using (StreamWriter EscribirF = new StreamWriter(archivo))
                {
                    EscribirF.Write(richTextBox1.Text);
                }
            }
            this.Text = "Mi Compilador - " + archivo;
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult Salir = MessageBox.Show(
                "¿Estás seguro que deseas salir?",
                "Confirmar salida",
                MessageBoxButtons.OKCancel);

            if (Salir == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private char Tipo_caracter(int caracter)
        {
            if ((caracter >= 65 && caracter <= 90) || (caracter >= 97 && caracter <= 122)) return 'l';
            else if (caracter >= 48 && caracter <= 57) return 'd';
            else
            {
                switch (caracter)
                {
                    case 10: return 'n';
                    case 34: return '"';
                    case 39: return 'c';
                    case 32: return 'e';
                    case 47: return '/';
                    default: return 's';
                }
            }
        }

        private void Simbolo()
        {
            char ch = (char)i_caracter;
            elemento = ch.ToString() + "\n";

            if (i_caracter == '!' ||
                (i_caracter >= 35 && i_caracter <= 38) ||
                (i_caracter >= 40 && i_caracter <= 45) ||
                i_caracter == 47 ||
                (i_caracter >= 58 && i_caracter <= 62) ||
                i_caracter == 91 || i_caracter == 93 ||
                i_caracter == 94 || i_caracter == 123 ||
                i_caracter == 124 || i_caracter == 125 ||
                i_caracter == ',' || i_caracter == ';' || i_caracter == '<' || i_caracter == '>' || i_caracter == '='
                )
            { }
            else
            {
                Error($"Símbolo inesperado '{ch}'");
            }
        }

        private void Cadena()
        {
            do
            {
                i_caracter = Leer.Read();
                if (i_caracter == 10) Numero_linea++;

            } while (i_caracter != 34 && i_caracter != -1);
            if (i_caracter == -1) Error(-1);
        }

        private void Caracter()
        {
            i_caracter = Leer.Read();
            if (i_caracter == -1) { Error("Carácter incompleto"); return; }
            i_caracter = Leer.Read();
            if (i_caracter != 39) Error(39);
        }

        private void Error(int i_caracter)
        {
            richTextBox2.AppendText("Error léxico " + (char)i_caracter + ", línea " + Numero_linea + "\n");
            N_error++;
            i_caracter = Leer.Read();
        }

        private void Error(string mensaje)
        {
            richTextBox2.AppendText("Error: " + mensaje + ", línea " + Numero_linea + "\n");
            N_error++;
        }

        private void Error(string tokenLocal, string esperado)
        {
            richTextBox2.AppendText($"Error: se esperaba '{esperado}', pero se encontró '{tokenLocal}', línea {Numero_linea}\n");
            N_error++;
        }

        private void Archivo_Libreria()
        {
            i_caracter = Leer.Read();
            if ((char)i_caracter == 'h') { Escribir.Write("libreria\n"); i_caracter = Leer.Read(); }
            else { Error(i_caracter); }
        }

        private bool Palabra_Reservada()
        {
            if (P_Reservadas.IndexOf(elemento.ToLower()) >= 0) return true;
            return false;
        }

        private void Identificador()
        {
            do
            {
                elemento = elemento + (char)i_caracter;
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'l' || Tipo_caracter(i_caracter) == 'd');

            if ((char)i_caracter == '.') { Archivo_Libreria(); }
            else
            {
                if (Palabra_Reservada()) Escribir.Write(elemento.ToLower() + "\n");
                else Escribir.Write("identificador\n");
            }
        }

        private void Numero_Real()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            Escribir.Write("numero_real\n");
        }

        private void Numero()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            if ((char)i_caracter == '.') { Numero_Real(); }
            else
            {
                Escribir.Write("numero\n");
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            analizarToolStripMenuItem.Enabled = true;
        }

        private bool Comentario()
        {
            i_caracter = Leer.Read();
            switch (i_caracter)
            {
                case 47:
                    do
                    {
                        i_caracter = Leer.Read();
                    } while (i_caracter != 10 && i_caracter != -1);
                    return true;
                case 42:
                    do
                    {
                        do
                        {
                            i_caracter = Leer.Read();
                            if (i_caracter == 10)
                            {
                                Numero_linea++;
                            }
                        } while (i_caracter != 42 && i_caracter != -1);
                        i_caracter = Leer.Read();
                    } while (i_caracter != 47 && i_caracter != -1);
                    if (i_caracter == -1)
                    {
                        Error(i_caracter);
                    }
                    i_caracter = Leer.Read();
                    return true;
                default: return false;
            }
        }

        private void analizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "";
            guardar();

            elemento = "";
            N_error = 0;
            Numero_linea = 1;

            if (archivo == null)
            {
                MessageBox.Show("No hay archivo cargado.");
                return;
            }

            archivoback = archivo.Remove(archivo.Length - 1) + "back";
            Escribir = new StreamWriter(archivoback);
            Leer = new StreamReader(archivo);

            i_caracter = Leer.Read();

            do
            {
                elemento = "";

                if ((char)i_caracter == '/')
                {
                    if (Comentario())
                    {
                        Escribir.Write("Comentario\n");
                        continue;
                    }
                }

                switch (Tipo_caracter(i_caracter))
                {
                    case 'l': Identificador(); break;
                    case 'd': Numero(); break;
                    case 's':
                        Simbolo();
                        Escribir.Write(elemento);
                        if (Tipo_caracter(i_caracter) == 's')
                            i_caracter = Leer.Read();
                        else
                            i_caracter = Leer.Read();
                        break;
                    case '"': Cadena(); Escribir.Write("Cadena\n"); i_caracter = Leer.Read(); break;
                    case 'c': Caracter(); Escribir.Write("Caracter\n"); i_caracter = Leer.Read(); break;
                    case 'n': i_caracter = Leer.Read(); Numero_linea++; Escribir.Write("LF\n"); break;
                    case 'e': i_caracter = Leer.Read(); break;
                    default: Error(i_caracter); break;
                }
            } while (i_caracter != -1);

            Escribir.Write("Fin\n");

            richTextBox2.AppendText("Errores: " + N_error + "\n");
            Escribir.Close();
            Leer.Close();
            AnalizadorSintactico();
        }

        private void Cabecera()
        {
            while (token != null && token != "Fin")
            {
                switch (token)
                {
                    case "#":
                        token = Leer.ReadLine();
                        if (token == "include" || token == "define")
                        {
                            Directiva_proc();
                        }
                        else
                        {
                            Error("Token inesperado después de '#'");
                        }
                        break;

                    case "LF":
                        Numero_linea++;
                        break;

                    case "int":
                    case "float":
                    case "double":
                    case "char":
                    case "void":
                        Declaracion();
                        break;

                    default:
                        if (token != null && token != "Fin")
                            Error($"Token inesperado en cabecera: '{token}'");
                        break;
                }
                token = Leer.ReadLine();
            }
        }

        private void AnalizadorSintactico()
        {
            Numero_linea = 1;
            Leer = new StreamReader(archivoback);
            token = Leer.ReadLine();
            Cabecera();
            Leer.Close();
        }

        private int Directiva_include()
        {
            if (token == "<")
            {
                token = Leer.ReadLine();
                if (token == "libreria")
                {
                    token = Leer.ReadLine();
                    if (token == ">")
                    {
                        return 1;
                    }
                    else
                    {
                        Error("Falta '>' en include");
                        return 0;
                    }
                }
                else
                {
                    Error("Falta nombre de librería");
                    return 0;
                }
            }
            else if (token == "Cadena" || token == "cadena")
            {
                return 1;
            }
            else
            {
                Error("Sintaxis de include inválida");
                return 0;
            }
        }

        private int Directiva_proc()
        {
            if (token == "include")
            {
                token = Leer.ReadLine();
                if (token == "<")
                {
                    token = Leer.ReadLine();
                    if (token == "libreria")
                    {
                        token = Leer.ReadLine();
                        if (token == ">")
                        {
                            return 1;
                        }
                        else
                        {
                            Error("Falta '>' en directiva include");
                            return 0;
                        }
                    }
                    else
                    {
                        Error("Falta nombre de librería entre '<' y '>'");
                        return 0;
                    }
                }
                else if (token == "Cadena")
                {
                    return 1;
                }
                else
                {
                    Error("Formato de include inválido");
                    return 0;
                }
            }
            else if (token == "define")
            {
                return 1;
            }
            else
            {
                Error("Se esperaba 'include' o 'define' después de '#'");
                return 0;
            }
        }

        private void Declaracion()
        {
            token = Leer.ReadLine();

            if (token == null)
            {
                Error("Declaración incompleta");
                return;
            }

            if (token == "identificador")
            {
                token = Leer.ReadLine();

                if (token == null)
                {
                    Error("Declaración incompleta después del identificador");
                    return;
                }

                if (token == "-")
                {
                    Error("Falta '=' en la asignación; se encontró '-' inmediatamente después del identificador");
                    while (token != null && token != ";" && token != "LF" && token != "Fin")
                    {
                        token = Leer.ReadLine();
                    }
                    if (token == ";") token = Leer.ReadLine();
                    return;
                }

                switch (token)
                {
                    case ";":
                        token = Leer.ReadLine();
                        return;

                    case "main":
                        do
                        {
                            token = Leer.ReadLine();
                            if (token == null || token == "Fin") break;
                        } while (token != "{");
                        return;

                    case "=":
                        Dec_VGlobal();
                        return;

                    case "[":
                        D_Arreglos();
                        if (token == "{")
                        {
                            Error("Falta '=' antes de la inicialización del arreglo");
                            int nivel = 1;
                            while ((token = Leer.ReadLine()) != null && token != "Fin" && nivel > 0)
                            {
                                if (token == "{") nivel++;
                                else if (token == "}") nivel--;
                            }
                            if (token == ";") token = Leer.ReadLine();
                        }
                        return;

                    case "(":
                        do
                        {
                            token = Leer.ReadLine();
                            if (token == null || token == "Fin") break;
                            if (token == "{") break;
                        } while (true);
                        token = Leer.ReadLine();
                        return;

                    default:
                        Error("Falta ';' en declaración o token inesperado después del identificador");
                        while (token != null && token != ";" && token != "LF" && token != "Fin")
                            token = Leer.ReadLine();
                        if (token == ";") token = Leer.ReadLine();
                        return;
                }
            }
            else if (token == "main")
            {
                do
                {
                    token = Leer.ReadLine();
                    if (token == null || token == "Fin") break;
                } while (token != "{");
            }
            else
            {
                Error("Falta identificador en declaración");
            }
        }

        private void D_Arreglos()
        {
            token = Leer.ReadLine();
            if (token == null) { Error("Declaración de arreglo incompleta"); return; }

            while (true)
            {
                if (token != "numero")
                {
                    Error("Se esperaba un valor de longitud de dimensión dentro de los corchetes");
                    return;
                }

                token = Leer.ReadLine();
                if (token != "]")
                {
                    Error("Se esperaba ']' después del número de la dimensión");
                    while (token != null && token != "]" && token != ";" && token != "Fin")
                        token = Leer.ReadLine();
                    if (token == "]") token = Leer.ReadLine();
                    else return;
                }
                else
                {
                    token = Leer.ReadLine();
                }

                if (token == "[")
                {
                    token = Leer.ReadLine();
                    continue;
                }
                break;
            }

            if (token == "{")
            {
                Error("Falta '=' antes de la inicialización del arreglo");

                int nivel = 1;
                string prevToken = null;
                while ((token = Leer.ReadLine()) != null && token != "Fin" && nivel > 0)
                {
                    if (token == "{")
                    {
                        if (prevToken == "numero" || prevToken == "identificador")
                            Error("Falta ',' entre elementos o subarreglos");
                        nivel++;
                    }
                    else if (token == "}")
                    {
                        nivel--;
                    }
                    else if (token == "numero" || token == "identificador")
                    {
                        if (prevToken == "numero" || prevToken == "identificador")
                            Error("Falta ',' entre valores dentro del arreglo");
                    }
                    prevToken = token;
                }
                return;
            }

            if (token == "=")
            {
                string prevToken = null;
                int nivel = 0;
                bool dentroArreglo = false;

                while ((token = Leer.ReadLine()) != null && token != "Fin")
                {
                    if (token == "{")
                    {
                        if (prevToken == "numero" || prevToken == "identificador" || prevToken == "}")
                            Error("Falta ',' entre subarreglos o elementos del arreglo");

                        nivel++;
                        dentroArreglo = true;
                    }
                    else if (token == "}")
                    {
                        nivel--;
                        if (nivel < 0) nivel = 0;
                        if (nivel == 0) dentroArreglo = false;
                    }
                    else if (token == "numero" || token == "numero_real" || token == "identificador" || token == "caracter")
                    {
                        if (prevToken == "numero" || prevToken == "numero_real" || prevToken == "identificador" || prevToken == "caracter")
                            Error("Falta ',' entre valores dentro del arreglo");
                    }
                    else if (token == ",")
                    {
                        if (prevToken == null || prevToken == "," || prevToken == "{")
                            Error("Coma inesperada en inicializador de arreglo");
                    }
                    else if (token == ";")
                    {
                        if (nivel > 0)
                        {
                            Error("Falta '}' antes de ';' en la inicialización del arreglo");
                        }
                        break;
                    }

                    prevToken = token;

                    if (!dentroArreglo && token == "}")
                        break;
                }

                if (nivel != 0)
                {
                    Error("Inicialización de arreglo sin '}' de cierre");
                    return;
                }

                if (token != ";")
                {
                    while (token != null && token != ";" && token != "Fin")
                        token = Leer.ReadLine();
                }
                token = Leer.ReadLine();
                return;
            }

            if (token != ";")
            {
                while (token != null && token != ";" && token != "Fin")
                    token = Leer.ReadLine();
                if (token != ";")
                    Error("Falta ';' al final de la declaración del arreglo");
            }

            token = Leer.ReadLine();
        }

        private int Constante()
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "-":
                    token = Leer.ReadLine();
                    if (token == "numero_real" || token == "numero" || token == "identificador") return 1;
                    else return 0;
                case "numero_real": return 1;
                case "numero": return 1;
                case "caracter": return 1;
                case "identificador": return 1;
                default: return 0;
            }
        }

        private void Dec_VGlobal()
        {
            token = Leer.ReadLine();
            if (token == null)
            {
                Error("Inicialización incompleta después de '='");
                return;
            }

            if (token == "-" || token == "numero" || token == "numero_real" || token == "identificador" || token == "caracter")
            {
                int ok = Constante();
                if (ok == 1)
                {
                    token = Leer.ReadLine();
                    while (token == "LF") token = Leer.ReadLine();
                    if (token == ";")
                    {
                        token = Leer.ReadLine();
                    }
                    else
                    {
                        Error(token, ";");
                    }
                }
                else
                {
                    Error("inicialización global válida", token);
                }
            }
            else if (token == "[")
            {
                D_Arreglos();
            }
            else if (token == ";")
            {
                token = Leer.ReadLine();
            }
            else
            {
                Error("Token inesperado en inicialización global: " + token);
                while (token != null && token != ";" && token != "Fin")
                    token = Leer.ReadLine();
                if (token == ";") token = Leer.ReadLine();
            }
        }
    }
}