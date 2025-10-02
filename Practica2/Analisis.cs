using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Practica2
{
    public partial class Form1 : Form
    {
        StreamWriter Escribir;
        StreamReader Leer;
        int i_caracter, N_error, Numero_linea;
        string archivo, archivoback;
        string elemento;
        string token;

        public Form1()
        {
            InitializeComponent();
            analizarToolStripMenuItem.Enabled = false;
        }

        private List<string> P_Reservadas = new List<string>()
        {
            "auto","break","case","char","const","continue","default","do","double","else","enum","extern",
            "float","for","goto","if","include","inline","int","long","main","register","restrict","return",
            "short","signed","sizeof","static","struct","switch","typedef","union","unsigned","void",
            "volatile","while","_Alignas","_Alignof","_Atomic","_Bool","_Complex","_Generic","_Imaginary",
            "_Noreturn","_Static_assert","_Thread_local"
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
            DialogResult Salir = MessageBox.Show("¿Estás seguro que deseas salir?", "Confirmar salida", MessageBoxButtons.OKCancel);
            if (Salir == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private char Tipo_caracter(int caracter)
        {
            if (caracter >= 65 && caracter <= 90 || caracter >= 97 && caracter <= 122) return 'l';
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
            if (i_caracter == 33 || i_caracter >= 35 && i_caracter <= 38 || i_caracter >= 40 && i_caracter <= 45 ||
                i_caracter == 47 || i_caracter >= 58 && i_caracter <= 62 || i_caracter == 91 || i_caracter == 93 ||
                i_caracter == 94 || i_caracter == 123 || i_caracter == 124 || i_caracter == 125)
            {
                elemento = ((char)i_caracter).ToString();
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

        private void Error(string mensaje)
        {
            richTextBox2.AppendText("Error sintáctico: " + mensaje + ", línea " + Numero_linea + "\n");
            N_error++;
        }

        private void Archivo_Libreria()
        {
            i_caracter = Leer.Read();
            if ((char)i_caracter == 'h') { elemento = "Archivo Libreria"; i_caracter = Leer.Read(); }
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

            if ((char)i_caracter == '.') { Archivo_Libreria(); }
            else
            {
                if (Palabra_Reservada()) elemento = "Palabra Reservada";
                else elemento = "Identificador";
            }
        }

        private void Numero_Real()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            elemento = "Numero real";
        }

        private void Numero()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            if ((char)i_caracter == '.') { Numero_Real(); }
            else { elemento = "Numero entero"; }
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
                    do { i_caracter = Leer.Read(); } while (i_caracter != 10);
                    return true;
                case 42:
                    do
                    {
                        do
                        {
                            i_caracter = Leer.Read();
                            if (i_caracter == 10) { Numero_linea++; }
                        } while (i_caracter != 42 && i_caracter != -1);
                        i_caracter = Leer.Read();
                    } while (i_caracter != 47 && i_caracter != -1);
                    if (i_caracter == -1) { Error(i_caracter); }
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
                        Escribir.WriteLine("Comentario");
                        continue;
                    }
                }
                switch (Tipo_caracter(i_caracter))
                {
                    case 'l': Identificador(); Escribir.WriteLine("Identificador: " + elemento); break;
                    case 'd': Numero(); Escribir.WriteLine("Número: " + elemento); break;
                    case 's': Simbolo(); Escribir.WriteLine("Símbolo especial: " + elemento); i_caracter = Leer.Read(); break;
                    case '"': Cadena(); Escribir.WriteLine("Cadena: " + elemento); i_caracter = Leer.Read(); break;
                    case 'c': Caracter(); Escribir.WriteLine("Caracter: " + elemento); i_caracter = Leer.Read(); break;
                    case 'n': i_caracter = Leer.Read(); Numero_linea++; break;
                    case 'e': i_caracter = Leer.Read(); break;
                    default: Error(i_caracter); break;
                }
            } while (i_caracter != -1);
            richTextBox2.AppendText("Errores: " + N_error + "\n");
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
                case "#": directivainclude_proc(); break;

                case "LF": Numero_linea++; token = Leer.ReadLine(); break;

                case "Tipo":
                    break;

                default: token = Leer.ReadLine(); break;
            }
        }

        private void AnalizadorSintactico()
        {
            if (File.Exists(archivoback))
            {
                Leer = new StreamReader(archivoback);
                Cabecera();
                Leer.Close();
            }
            else
            {
                Error("Archivo de respaldo no encontrado");
            }
        }

        private void directiva_include()
        {
            token = Leer.ReadLine();
            if (token == null) return;

            if (token == "include")
            {
                token = Leer.ReadLine();
                if (token == null) return;

                if (token == "<")
                {
                    token = Leer.ReadLine();
                    if (token == null) return;

                    if (token == "Libreria")
                    {
                        token = Leer.ReadLine();
                        if (token == ">")
                        {
                            token = Leer.ReadLine();
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
                else if (token == "cadena")
                {
                    token = Leer.ReadLine();
                    return;
                }
                else
                {
                    Error("Formato de include no reconocido");
                    return;
                }
            }
            else
            {
                Error("Se esperaba 'include' después de '#'");
                return;
            }
        }

        private void directiva_proc()
        {
            if (token == null)
            {
                token = Leer.ReadLine();
                if (token == null) return;
            }

            switch (token)
            {
                case "include":
                    directiva_include();
                    break;

                case "define":
                    token = Leer.ReadLine();
                    return;

                default:
                    Error("Directiva de procesador desconocida: " + token);
                    return;
            }
        }

        private void Declaracion()
        {
            string tipo = elemento;
            LeerToken();
            if (token == "Identificador")
            {
                string nombre = elemento;
                LeerToken();
                if (token == "Símbolo especial" && elemento == ";")
                {
                    richTextBox2.AppendText("Declaración: " + tipo + " " + nombre + ";\n");
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
