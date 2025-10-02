using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

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
            "inline", "int", "long", "register", "restrict", "return", "short",
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
                using (StreamReader Leer = new StreamReader(archivo))
                {
                    richTextBox1.Text = Leer.ReadToEnd();
                }

            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
            analizarToolStripMenuItem.Enabled = true;
            //habilita la opcion compilar cuando se carga un archivo.
        }
        private void guardar()
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog();
            VentanaGuardar.Filter = "Texto|*.c";
            if (archivo != null)
            {
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text);
                }
            }
            else
            {
                if (VentanaGuardar.ShowDialog() == DialogResult.OK)
                {
                    archivo = VentanaGuardar.FileName;
                    using (StreamWriter Escribir = new StreamWriter(archivo))
                    {
                        Escribir.Write(richTextBox1.Text);
                    }
                }
            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
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
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text);
                }
            }
            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
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
            if (caracter >= 65 && caracter <= 90 || caracter >= 97 && caracter <= 122) { return 'l'; } //letra 
            else
            {
                if (caracter >= 48 && caracter <= 57) { return 'd'; } //digito 
                else
                {
                    switch (caracter)
                    {
                        case 10: return 'n'; //salto de linea
                        case 34: return '"'; //inicio de cadena
                        case 39: return 'c'; //inicio de caracter
                        case 32: return 'e'; //espacio
                        case 47: return '/';

                        default: return 's';//simbolo
                    }
                    ;

                }
            }

        }
        private void Simbolo()
        {
            if (i_caracter == 33 ||
                i_caracter >= 35 && i_caracter <= 38 ||
                i_caracter >= 40 && i_caracter <= 46 || // ahora incluye '.' (46)
                i_caracter == 47 ||
                i_caracter >= 58 && i_caracter <= 62 ||
                i_caracter == 91 ||
                i_caracter == 93 ||
                i_caracter == 94 ||
                i_caracter == 123 ||
                i_caracter == 124 ||
                i_caracter == 125
                )
            {
                elemento = ((char)i_caracter).ToString() + " Símbolo\n";
            }
            else
            {
                Error(i_caracter);
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
            i_caracter = Leer.Read();
            if (i_caracter != 39) Error(39);
        }

        private void Error(int caracter)
        {
            richTextBox2.AppendText("Error léxico: " + (char)caracter + " en línea " + Numero_linea + "\n");
            N_error++;
        }

        private void Error(string mensaje)
        {
            richTextBox2.AppendText("Error sintáctico: " + mensaje + " en línea " + Numero_linea + "\n");
            N_error++;
        }

        private void Archivo_Libreria()
        {
            string nombre = "";
            // i_caracter ya está posicionado en el primer caracter después de '<'
            while (i_caracter != -1 && (char)i_caracter != '>')
            {
                nombre += (char)i_caracter;
                i_caracter = Leer.Read();
            }

            if (i_caracter == '>')
            {
                elemento = nombre.Trim();
                Escribir.WriteLine($"Archivo {elemento}"); // registra Archivo stdio.h
                                                           // NOTA: no hacemos i_caracter = Leer.Read() aquí, lo hace el llamador después de escribir "Simbolo >"
            }
            else
            {
                Error("Se esperaba '>' al final de archivo de librería");
            }
        }



        private bool Palabra_Reservada()
        {
            if (P_Reservadas.IndexOf(elemento) >= 0) return true;
            return false;
        }
        private void Identificador()
        {
            string lex = "";
            do
            {
                lex += (char)i_caracter;
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'l' || Tipo_caracter(i_caracter) == 'd');

            elemento = lex;

            if (Palabra_Reservada())
            {
                Escribir.WriteLine($"PalabraReservada {elemento}");
            }
            else
            {
                Escribir.WriteLine($"Identificador {elemento}");
            }
        }



        private void Numero_Real()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            elemento = "Numero real\n";
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
                elemento = "Numero entero\n";
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
                    } while (i_caracter != 10);
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
            guardar(); elemento = "";
            N_error = 0; Numero_linea = 1;
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
                        Escribir.WriteLine("Comentario\n");
                        continue;
                    }
                }
                switch (Tipo_caracter(i_caracter))
                {
                    case 'l':
                        Identificador();
                        break;

                    case 'd':
                        Numero();
                        Escribir.WriteLine($"Numero {elemento}"); // "Numero 123"
                        break;

                    case 's':
                        if ((char)i_caracter == '<')
                        {
                            // inicio de <archivo>
                            Escribir.WriteLine("Simbolo <");
                            i_caracter = Leer.Read(); // avanzar al contenido dentro de < >
                            Archivo_Libreria();       // escribe "Archivo nombre.ext"
                            if (i_caracter == '>')
                            {
                                Escribir.WriteLine("Simbolo >");
                                i_caracter = Leer.Read(); // avanzar después de '>'
                            }
                            else
                            {
                                Error("Se esperaba '>' al final de archivo de librería");
                            }
                        }
                        else
                        {
                            Simbolo();
                            Escribir.WriteLine($"Simbolo {elemento.Trim()}");
                            i_caracter = Leer.Read();
                        }
                        break;


                    case '"': Cadena(); Escribir.WriteLine("Cadena\n"); i_caracter = Leer.Read(); break;
                    case 'c': Caracter(); Escribir.WriteLine("Caracter\n"); i_caracter = Leer.Read(); break;
                    case 'n': i_caracter = Leer.Read(); Numero_linea++; break;
                    case 'e': i_caracter = Leer.Read(); break;
                    default: Error(i_caracter); break;
                }
                ;

            } while (i_caracter != -1);

            richTextBox2.AppendText("Errores: " + N_error);
            Escribir.Close();
            Leer.Close();
            AnalizadorSintactico();
        }
        private void directivainclude_proc()
        {
            directiva_include();
        }

        private void Cabecera()
        {
            token = Leer.ReadLine();

            if (token == null) return;

            switch (token)
            {
                case "PalabraReservada":
                    if (lexema == "int" || lexema == "float" || lexema == "char")
                    {
                        Declaracion();
                    }
                    break;
                case "#": directivainclude_proc(); break;

                case "LF": Numero_linea++; token = Leer.ReadLine(); break;

                case "Tipo":
                    break;

                default: token = Leer.ReadLine(); break;
            }
        }

        private void AnalizadorSintactico()
        {
            if (!File.Exists(archivoback))
            {
                Error("Archivo de respaldo no encontrado");
                return;
            }

            Leer = new StreamReader(archivoback);
            NextToken();

            while (token != null)
            {
                if (token == "PalabraReservada")
                {
                    if (lexema == "int" || lexema == "float" || lexema == "char")
                    {
                        Declaracion();
                    }
                    else
                    {
                        NextToken();
                    }
                }
                else if (token == "Simbolo" && lexema == "#")
                {
                    directiva_proc();
                }
                else
                {
                    NextToken();
                }
            }


            Leer.Close();
        }


        private void directiva_include()
        {
            // Aquí asumimos que lexema == "include" (directiva_proc ya avanzó)
            if (lexema != "include")
            {
                Error("Se esperaba 'include' después de '#'");
                return;
            }

            // Avanzar para leer lo que sigue a 'include' (debe ser '<' o una cadena)
            NextToken();
            if (token == null) return;

            // Caso: #include <archivo>
            if (tipoToken == "Simbolo" && lexema == "<")
            {
                // leer nombre de archivo (tipoToken debe ser "Archivo")
                NextToken();
                if (token == null) return;

                if (tipoToken == "Archivo")
                {
                    // ahora esperamos el símbolo '>'
                    NextToken();
                    if (token == null) return;

                    if (tipoToken == "Simbolo" && lexema == ">")
                    {
                        NextToken(); // avanzar después de '>'
                        return;
                    }
                    else
                    {
                        Error("\">\" esperado en directiva include");
                        return;
                    }
                }
                else
                {
                    Error("Nombre de librería esperado");
                    return;
                }
            }
            // Caso: #include "archivo.h"
            else if (tipoToken == "Cadena")
            {
                NextToken(); // aceptar la cadena y seguir
                return;
            }
            else
            {
                Error("Formato de include no reconocido");
                return;
            }
        }



        private void directiva_proc()
        {
            NextToken();
            if (token == null) return;

            switch (lexema)
            {
                case "include":
                    directiva_include();
                    break;
                case "define":
                    NextToken(); // saltar nombre de macro
                    break;
                default:
                    Error("Directiva de procesador desconocida: " + lexema);
                    break;
            }
        }



        private void NextToken()
        {
            string linea = Leer.ReadLine();
            if (linea == null)
            {
                token = null;
                return;
            }

            string[] partes = linea.Split(' ');
            tipoToken = partes[0].Trim();
            lexema = partes.Length > 1 ? partes[1].Trim() : "";
            token = tipoToken; // compatibilidad con parser
        }



        private void Declaracion()
        {
            string tipo = lexema; // int, float, etc.
            NextToken();

            if (token == "Identificador")
            {
                string nombre = lexema;
                NextToken();

                if (token == "Simbolo" && lexema == "(")
                {
                    // Es una función, no variable
                    richTextBox2.AppendText($"Declaración de función: {tipo} {nombre}()\n");

                    // Saltar parámetros hasta ')'
                    while (!(token == "Simbolo" && lexema == ")") && token != null)
                    {
                        NextToken();
                    }
                    NextToken(); // leer el '{' después de ')'
                }
                else if (token == "Simbolo" && lexema == ";")
                {
                    // Declaración normal de variable
                    richTextBox2.AppendText($"Declaración: {tipo} {nombre};\n");
                    NextToken();
                }
                else
                {
                    Error("Falta ';' en declaración");
                }
            }
            else
            {
                Error("Se esperaba un identificador en declaración");
            }
        }


    }
}