using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
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
            if (caracter >= 65 && caracter <= 90 || caracter >= 97 && caracter <= 122) return 'l'; // Letra
            else if (caracter >= 48 && caracter <= 57) return 'd'; // Dígito
            else
            {
                switch (caracter)
                {
                    case 10: return 'n'; // Salto de línea
                    case 34: return '"'; // Comillas dobles
                    case 39: return 'c'; // Comilla simple
                    case 32: return 'e'; // Espacio
                    case 47: return '/'; // Posible comentario
                    default: return 's'; // Otro símbolo
                }
            }
        }
        private void Simbolo()
        {
            if (i_caracter == 33 ||
                i_caracter >= 35 && i_caracter <= 38 ||
                i_caracter >= 40 && i_caracter <= 45 ||
                i_caracter == 47 ||
                i_caracter >= 58 && i_caracter <= 62 ||
                i_caracter == 91 || i_caracter == 93 ||
                i_caracter == 94 || i_caracter == 123 ||
                i_caracter == 124 || i_caracter == 125)
            {
                elemento = ((char)i_caracter).ToString() + " Símbolo\n";
            }
            else { Error(i_caracter); }
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

        private void Error(int i_caracter)
        {
            richTextBox2.AppendText("Error léxico " + (char)i_caracter + ", línea " + Numero_linea + "\n");
            N_error++;
        }

        private void Archivo_Libreria()
        {
            i_caracter = Leer.Read();
            if ((char)i_caracter == 'h') { elemento = "Archivo Libreria\n"; i_caracter = Leer.Read(); }
            else { Error(i_caracter); }
        }

        private bool Palabra_Reservada()
        {
            if (P_Reservadas.IndexOf(elemento) >= 0) return true;
            return false;
        }
        private void Identificador()
        {
            do
            {
                elemento = elemento + (char)i_caracter;
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'l' || Tipo_caracter(i_caracter) == 'd');

            if ((char)i_caracter == '.') { Archivo_Libreria(); } // Si es .h
            else
            {
                if (Palabra_Reservada()) elemento = "Palabra Reservada\n";
                else elemento = "Identificador\n";
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
            guardar();

            elemento = "";
            N_error = 0;
            Numero_linea = 1;

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
                    case 'l': Identificador(); Escribir.Write(elemento); break;
                    case 'd': Numero(); Escribir.Write(elemento); break;
                    case 's': Simbolo(); Escribir.Write(elemento); i_caracter = Leer.Read(); break;
                    case '"': Cadena(); Escribir.Write("Cadena\n"); i_caracter = Leer.Read(); break;
                    case 'c': Caracter(); Escribir.Write("Caracter\n"); i_caracter = Leer.Read(); break;
                    case 'n': i_caracter = Leer.Read(); Numero_linea++; Escribir.Write("LF\n"); break;
                    case 'e': i_caracter = Leer.Read(); break;
                    default: Error(i_caracter); break;
                }
            } while (i_caracter != -1);

            Escribir.Write("Fin\n");

            richTextBox2.AppendText("Errores: " + N_error);
            Escribir.Close();
            Leer.Close();
        }
        /**private void directivainclude_proc()
        {
            Directiva_include();
        }

        private void Cabecera()
        {
            token = Leer.ReadLine();
            switch (token)
            {
                case "#": Directiva_proc(); break;
                case "LF": Numero_linea++; token = Leer.ReadLine(); break;
                case "Tipo": Declaracion(); break;
            }
        }

        private void AnalizadorSintactico()
        {
            Numero_linea = 0;
            Leer = new StreamReader(archivoback);
            Cabecera();
        }


        private int Directiva_include()
        {
            token = Leer.ReadLine();
            if (token == "include")
            {
                token = Leer.ReadLine();
                switch (token)
                {
                    case "<":
                        token = Leer.ReadLine();
                        if (token == "libreria")
                        {
                            token = Leer.ReadLine();
                            if (token == ">")
                            {
                                token = Leer.ReadLine();
                            }
                            else
                            {
                                Error(">"); return 0;
                            }
                        }
                        else
                        {
                            Error(token); return 0;
                        }
                        break;
                    case "cadena": token = Leer.ReadLine(); return 0;
                    default: Error("Libreria"); return 0;
                }
            }
            return 1;
        }

        private int Directiva_proc()
        {
            switch (token)
            {
                case "include": Directiva_include(); break;
                case "define":
                default: Error("Directiva de procesador"); return 0;
            }
            return 1;
        }



        private void Declaracion()
        {

        }**/
    }
}