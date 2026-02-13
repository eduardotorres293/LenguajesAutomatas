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
                        Avanzar();
                        if (token == "include" || token == "define") Directiva_proc();
                        else { Error("Error en directiva"); Avanzar(); }
                        break;

                    case "LF":
                        Numero_linea++;
                        Avanzar();
                        break;

                    case "int":
                    case "float":
                    case "double":
                    case "char":
                    case "void":
                        Declaracion();
                        break;

                    case "main":
                        Error("La función 'main' debe tener un tipo de retorno (ej. int main)");
                        Procesar_Main();
                        break;

                    default:
                        if (token != null && token != "Fin")
                        {
                            Error($"Token inesperado en cabecera: '{token}'");
                            Avanzar();
                        }
                        break;
                }
            }
        }

        private void AnalizadorSintactico()
        {
            Numero_linea = 1;
            Leer = new StreamReader(archivoback);
            Avanzar();
            Cabecera();
            Leer.Close();
        }

        private int Directiva_proc()
        {
            if (token == "include")
            {
                Avanzar();

                if (token == "<")
                {
                    do
                    {
                        Avanzar();
                        if (token == "Fin" || token == "LF")
                        {
                            Error("Falta '>' al final del include");
                            return 0;
                        }
                    } while (token != ">");

                    Avanzar();
                    return 1;
                }
                else if (token == "Cadena")
                {
                    Avanzar();
                    return 1;
                }
                else
                {
                    Error("Se esperaba '<libreria>' o \"archivo\" después de include");
                    return 0;
                }
            }
            else if (token == "define")
            {
                Avanzar();
                while (token != "LF" && token != "Fin") Avanzar();
                return 1;
            }

            return 0;
        }

        private void Declaracion()
        {
            Avanzar();

            if (token == null) { Error("Declaración incompleta"); return; }

            if (token == "main")
            {
                Procesar_Main();
                return;
            }

            if (token == "identificador")
            {
                Avanzar();

                if (token == null) { Error("Declaración incompleta"); return; }

                if (token == "-")
                {
                    Error("Token inesperado '-'. ¿Quisiste usar '='?");
                    while (token != ";" && token != "LF" && token != "Fin") Avanzar();
                    if (token == ";") Avanzar();
                    return;
                }

                switch (token)
                {
                    case ";":
                        Avanzar();
                        return;

                    case "=":
                        Dec_VGlobal();
                        return;

                    case "[":
                        D_Arreglos();
                        return;

                    case "(":
                        Definicion_Funcion();
                        return;

                    default:
                        if (token == "numero" || token == "numero_real" || token == "caracter" || token == "identificador")
                        {
                            Error($"Falta el signo '=' antes de '{token}'");
                            Avanzar();
                            if (token == ";") Avanzar();
                        }
                        else
                        {
                            Error($"Se esperaba ';' o '=', pero se encontró '{token}'");
                            if (token != "Fin" && token != "}") Avanzar();
                        }
                        return;
                }
            }
            else
            {
                Error("Se esperaba un identificador después del tipo de dato");
                Avanzar();
            }
        }


        private void D_Arreglos()
        {
            Avanzar();

            while (true)
            {
                if (token != "numero" && token != "identificador")
                {
                    Error("Se esperaba tamaño del arreglo");
                    return;
                }
                Avanzar();

                if (token != "]")
                {
                    Error("Falta ']'");
                    return;
                }
                Avanzar();

                if (token == "[")
                {
                    Avanzar();
                    continue;
                }
                break;
            }

            if (token == "=")
            {
                Avanzar();
                if (token != "{")
                {
                    Error("Se esperaba '{' para iniciar la inicialización");
                    return;
                }

                int balance = 0;
                bool primerElemento = true;

                balance = 1;
                Avanzar();

                while (balance > 0 && token != "Fin" && token != ";")
                {
                    if (token == "{")
                    {
                        if (!primerElemento) Error("Falta ',' entre sub-arreglos");
                        balance++;
                        primerElemento = true;
                        Avanzar();
                    }
                    else if (token == "}")
                    {
                        balance--;
                        primerElemento = false;
                        Avanzar();
                    }
                    else if (token == ",")
                    {
                        if (primerElemento) Error("Coma inesperada al inicio de bloque");
                        primerElemento = true;
                        Avanzar();
                    }
                    else if (token == "numero" || token == "identificador" || token == "numero_real" || token == "caracter")
                    {
                        if (!primerElemento) Error("Falta ',' entre valores");

                        primerElemento = false;
                        Avanzar();
                    }
                    else
                    {
                        Error("Token inesperado en inicialización: " + token);
                        Avanzar();
                    }
                }

                if (balance > 0) Error("Falta '}' de cierre en la inicialización");
            }

            if (token != ";")
            {
                Error("Falta ';' al final del arreglo");
            }
            else
            {
                Avanzar();
            }
        }

        private int Constante()
        {
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
            Avanzar();

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
                    Avanzar();

                    while (token == "LF") Avanzar();

                    if (token == ";")
                    {
                        Avanzar();
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
                Avanzar();
            }
            else
            {
                Error("Token inesperado en inicialización global: " + token);

                while (token != null && token != ";" && token != "Fin")
                    token = Leer.ReadLine();

                if (token == ";") Avanzar();
            }
        }

        private void Avanzar()
        {
            do
            {
                token = Leer.ReadLine();
                if (token == "LF") Numero_linea++;
            } while (token == "LF");
        }

        private void Bloque_Codigo()
        {
            Avanzar();

            while (token != "}" && token != "Fin" && token != null)
            {
                switch (token)
                {
                    case "int":
                    case "float":
                    case "double":
                    case "char":
                    case "bool":
                        Declaracion();
                        break;
                    case "if": Estructura_If(); break;
                    case "while": Estructura_While(); break;
                    case "for": Estructura_For(); break;
                    case "switch": Estructura_Switch(); break;

                    case "else":
                        Error("Error de sintaxis: Se encontró un 'else' inesperado. Posiblemente falta una llave de cierre '}' en el bloque anterior.");
                        return;

                    case "identificador":
                        while (token != ";" && token != "Fin") Avanzar();
                        Avanzar();
                        break;

                    case "LF":
                        Avanzar();
                        break;

                    default:
                        Avanzar();
                        break;
                }
            }

            if (token == "}")
            {
                Avanzar();
            }
            else if (token == "Fin")
            {
                Error("Falta la llave de cierre '}' al final del bloque");
            }
        }

        private void Estructura_If()
        {
            Avanzar();
            if (token != "(") Error("Se esperaba '(' después de 'if'");

            Validar_Expresion_Parentesis();

            if (token == "{")
            {
                Bloque_Codigo();
            }
            else
            {
                Error("Se esperaba '{' después de la condición del if");
                while (token != ";" && token != "Fin") Avanzar();
                if (token == ";") Avanzar();
            }

            if (token == "else")
            {
                Avanzar();
                if (token == "{")
                {
                    Bloque_Codigo();
                }
                else if (token == "if")
                {
                    Estructura_If();
                }
                else
                {
                    Error("Se esperaba '{' o 'if' después de 'else'");
                }
            }
        }
        private void Estructura_While()
        {
            Avanzar();
            if (token != "(") Error("Se esperaba '(' después de 'while'");

            Validar_Expresion_Parentesis();

            if (token != "{") Error("Se esperaba '{' después de la condición del while");

            Bloque_Codigo();
        }
        private void Estructura_For()
        {
            Avanzar();
            if (token != "(") Error("Se esperaba '(' después de 'for'");
            Avanzar();

            while (token != ";" && token != "Fin") Avanzar();
            if (token != ";") Error("Falta ';' en la inicialización del for");
            Avanzar();

            while (token != ";" && token != "Fin") Avanzar();
            if (token != ";") Error("Falta ';' en la condición del for");
            Avanzar();

            while (token != ")" && token != "Fin") Avanzar();

            if (token != ")") Error("Se esperaba ')' al finalizar el for");
            Avanzar();

            if (token != "{") Error("Se esperaba '{' después de los parámetros del for");

            Bloque_Codigo();
        }
        private void Estructura_Switch()
        {
            Avanzar();
            if (token != "(") Error("Se esperaba '(' después de 'switch'");
            Validar_Expresion_Parentesis();

            if (token != "{") Error("Se esperaba '{' para abrir el switch");
            Avanzar();

            while (token != "}" && token != "Fin")
            {
                if (token == "case")
                {
                    Avanzar();
                    if (token == "numero" || token == "caracter" || token == "identificador")
                        Avanzar();
                    else
                        Error("Se esperaba un valor constante para el 'case'");

                    if (token != ":") Error("Se esperaba ':' después del valor del case");
                    Avanzar();

                    while (token != "break" && token != "case" && token != "default" && token != "}" && token != "Fin")
                    {
                        Avanzar();
                        if (token == ";") Avanzar();
                    }

                    if (token == "break")
                    {
                        Avanzar();
                        if (token == ";") Avanzar();
                        else Error("Falta ';' después del break");
                    }
                }
                else if (token == "default")
                {
                    Avanzar();
                    if (token != ":") Error("Se esperaba ':' después de default");
                    Avanzar();
                    while (token != "}" && token != "break" && token != "Fin") Avanzar();
                    if (token == "break")
                    {
                        Avanzar();
                        if (token == ";") Avanzar();
                    }
                }
                else if (token == "LF")
                {
                    Avanzar();
                }
                else
                {
                    Avanzar();
                }
            }

            if (token == "}") Avanzar();
        }
        private void Validar_Expresion_Parentesis()
        {
            int balance = 1;
            Avanzar();

            while (balance > 0 && token != "Fin")
            {
                if (token == "(") balance++;
                else if (token == ")") balance--;

                if (balance > 0) Avanzar();
            }

            if (balance == 0) Avanzar();
            else Error("Paréntesis no balanceados en la expresión");
        }

        private void Procesar_Main()
        {
            Avanzar();
            if (token != "(")
            {
                Error("Falta '(' después de main");
            }
            else
            {
                Avanzar();
            }

            if (token != ")")
            {
                Error("Falta ')' en main");
                while (token != ")" && token != "{" && token != "Fin") Avanzar();
                if (token == ")") Avanzar();
            }
            else
            {
                Avanzar();
            }

            if (token == "{")
            {
                Bloque_Codigo();
            }
            else
            {
                Error("Falta '{' para iniciar el cuerpo del main");
            }
        }
        private void Definicion_Funcion()
        {
            Avanzar();

            if (token == ")")
            {
                Avanzar();
            }
            else
            {
                while (true)
                {
                    if (token != "int" && token != "float" && token != "double" &&
                        token != "char" && token != "void")
                    {
                        Error("Se esperaba un tipo de dato en los parámetros");
                        return;
                    }

                    Avanzar();

                    if (token != "identificador")
                    {
                        Error("Se esperaba un identificador como nombre del parámetro");
                        return;
                    }

                    Avanzar();

                    if (token == ",")
                    {
                        Avanzar();
                        continue;
                    }
                    else if (token == ")")
                    {
                        Avanzar();
                        break;
                    }
                    else
                    {
                        Error("Se esperaba ',' o ')'");
                        return;
                    }
                }
            }

            if (token == "{")
            {
                Bloque_Codigo();
            }
            else
            {
                Error("Se esperaba '{' para iniciar el cuerpo de la función");
            }
        }
    }
}