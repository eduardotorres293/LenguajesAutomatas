using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Practica2
{
    public partial class Form1 : Form
    {
        private readonly List<string> P_Reservadas = new List<string>
        {
            "auto", "break", "case", "char", "const", "continue", "default", "do",
            "double", "else", "enum", "extern", "float", "for", "goto", "if", "include",
            "inline", "int", "long", "main", "register", "restrict", "return", "short",
            "signed", "sizeof", "static", "struct", "switch", "typedef", "union",
            "unsigned", "void", "volatile", "while", "_Alignas", "_Alignof",
            "_Atomic", "_Bool", "_Complex", "_Generic", "_Imaginary", "_Noreturn",
            "_Static_assert", "_Thread_local"
        };

        private List<SimboloFuncion> TablaFunciones = new List<SimboloFuncion>();
        private Stack<List<SimboloVariable>> PilaAmbitos = new Stack<List<SimboloVariable>>();

        // ── Nodo del árbol de expresiones ────────────────────────────────────
        class NodoExpresion
        {
            public string Valor;
            public NodoExpresion Izquierdo;
            public NodoExpresion Derecho;

            public NodoExpresion(string valor, NodoExpresion izq = null, NodoExpresion der = null)
            {
                Valor = valor;
                Izquierdo = izq;
                Derecho = der;
            }
        }

        class SimboloVariable
        {
            public string Nombre;
            public string Tipo;
            public int Direccion;

            public SimboloVariable(string nombre, string tipo, int direccion)
            {
                Nombre = nombre;
                Tipo = tipo;
                Direccion = direccion;
            }
        }

        class SimboloFuncion
        {
            public string Nombre;
            public string TipoRetorno;
            public int NumeroParametros;
            public List<string> TiposParametros;

            public SimboloFuncion(string nombre, string tipoRetorno)
            {
                Nombre = nombre;
                TipoRetorno = tipoRetorno;
                NumeroParametros = 0;
                TiposParametros = new List<string>();
            }
        }

        public Form1()
        {
            InitializeComponent();
            EntrarAmbito();
            TablaFunciones.Add(new SimboloFuncion("printf", "int"));
            analizarToolStripMenuItem.Enabled = false;
        }

        private void EntrarAmbito() => PilaAmbitos.Push(new List<SimboloVariable>());

        private void SalirAmbito()
        {
            if (PilaAmbitos.Count > 1)
                PilaAmbitos.Pop();
        }

        private bool ExisteVariableEnAmbitoActual(string nombre)
        {
            return PilaAmbitos.Count > 0 && PilaAmbitos.Peek().Any(v => v.Nombre == nombre);
        }

        private bool VariableDeclarada(string nombre)
        {
            return PilaAmbitos.Any(ambito => ambito.Any(v => v.Nombre == nombre));
        }

        private bool ExisteFuncion(string nombre)
        {
            return TablaFunciones.Any(f => f.Nombre == nombre);
        }

        private void AgregarVariable(string nombre)
        {
            PilaAmbitos.Peek().Add(new SimboloVariable(nombre, tipoActual, direccionActual));
            direccionActual += 4;
        }

        // ════════════════════════════════════════════════════════════════════
        //  ÁRBOL DE EXPRESIONES
        // ════════════════════════════════════════════════════════════════════

        // Imprime el árbol con indentación usando └── para cada nodo hijo
        private void ImprimirArbol(NodoExpresion nodo, string prefijo = "", bool esRaiz = true)
        {
            if (nodo == null) return;

            if (esRaiz)
                richTextBox2.AppendText($"  Árbol: {nodo.Valor}\n");
            else
                richTextBox2.AppendText($"{prefijo}└── {nodo.Valor}\n");

            string nuevoPrefijo = esRaiz ? "      " : prefijo + "    ";
            ImprimirArbol(nodo.Izquierdo, nuevoPrefijo, false);
            ImprimirArbol(nodo.Derecho, nuevoPrefijo, false);
        }

        // ════════════════════════════════════════════════════════════════════
        //  ANALIZADOR SINTÁCTICO DE EXPRESIONES
        //
        //  Gramática (basada en el diagrama del pizarrón):
        //
        //  Expresion  →  Operando { Operador Operando }
        //             |  ( Expresion )
        //
        //  Operando   →  [++ | --] identificador [++ | --]   (prefijo/postfijo)
        //             |  identificador ( [Expresion {, Expresion}] )  (llamada)
        //             |  numero
        //             |  numero_real
        //             |  caracter
        //             |  true | false                        (booleano)
        //             |  ( Expresion )
        //
        //  Operador   →  + | - | * | / | %
        //             |  ++ | --
        //             |  == | != | < | > | <= | >=
        //             |  && | ||
        // ════════════════════════════════════════════════════════════════════

        // Devuelve true si el token actual puede ser el inicio de un operando
        private bool EsInicioOperando()
        {
            return token == "numero" ||
                   token == "numero_real" ||
                   token == "caracter" ||
                   token == "Cadena" ||
                   token == "identificador" ||
                   token == "true" ||
                   token == "false" ||
                   token == "++" ||
                   token == "--" ||
                   token == "(";
        }

        // Devuelve true si el token actual es un operador binario o unario listado en el pizarrón
        private bool EsOperador()
        {
            return token == "+" || token == "-" ||
                   token == "*" || token == "/" || token == "%" ||
                   token == "++" || token == "--" ||
                   token == "==" || token == "!=" ||
                   token == "<" || token == ">" ||
                   token == "<=" || token == ">=" ||
                   token == "&&" || token == "||";
        }

        // ── Operando ─────────────────────────────────────────────────────────
        // Reconoce: prefijo++/--, identificador, postfijo++/--, llamada a función,
        //           número, número_real, carácter, booleano, (Expresion)
        private NodoExpresion Operando()
        {
            // Operador unario prefijo: ++ o --
            if (token == "++" || token == "--")
            {
                string opPrefijo = token;
                Avanzar();

                if (token != "identificador")
                {
                    Error($"Se esperaba un identificador después de '{opPrefijo}'");
                    return new NodoExpresion("?");
                }

                string nombrePre = valorToken;
                if (!VariableDeclarada(nombrePre))
                    Error($"La variable '{nombrePre}' no ha sido declarada");

                Avanzar();
                return new NodoExpresion(opPrefijo + nombrePre);
            }

            // Número entero
            if (token == "numero")
            {
                NodoExpresion n = new NodoExpresion("numero");
                Avanzar();
                return n;
            }

            // Número real
            if (token == "numero_real")
            {
                NodoExpresion n = new NodoExpresion("numero_real");
                Avanzar();
                return n;
            }

            // Carácter literal
            if (token == "caracter")
            {
                NodoExpresion n = new NodoExpresion("caracter");
                Avanzar();
                return n;
            }

            // Cadena de texto: "..."
            if (token == "Cadena")
            {
                NodoExpresion n = new NodoExpresion("Cadena");
                Avanzar();
                return n;
            }

            // Booleano: true o false
            if (token == "true" || token == "false")
            {
                NodoExpresion n = new NodoExpresion(token);
                Avanzar();
                return n;
            }

            // Identificador: variable, postfijo o llamada a función
            if (token == "identificador")
            {
                string nombre = valorToken;
                Avanzar();

                // Llamada a función: identificador (  ...  )
                if (token == "(")
                {
                    if (!ExisteFuncion(nombre))
                        Error($"La función '{nombre}' no ha sido declarada");

                    NodoExpresion nodoLlamada = new NodoExpresion($"func:{nombre}");
                    Avanzar();

                    if (token != ")")
                    {
                        // Primer argumento
                        NodoExpresion primerArg = Expresion();
                        nodoLlamada.Izquierdo = primerArg;
                        NodoExpresion actual = nodoLlamada;

                        while (token == ",")
                        {
                            Avanzar();
                            NodoExpresion argExtra = Expresion();
                            actual.Derecho = new NodoExpresion(",", argExtra, null);
                            actual = actual.Derecho;
                        }
                    }

                    if (token != ")")
                        Error("Falta ')' al cerrar la llamada a función");
                    else
                        Avanzar();

                    return nodoLlamada;
                }

                // Postfijo: identificador ++  o  identificador --
                if (token == "++" || token == "--")
                {
                    string opPostfijo = token;
                    if (!VariableDeclarada(nombre))
                        Error($"La variable '{nombre}' no ha sido declarada");
                    Avanzar();
                    return new NodoExpresion(nombre + opPostfijo);
                }

                // Variable simple
                if (!VariableDeclarada(nombre) && !ExisteFuncion(nombre))
                    Error($"La variable '{nombre}' no ha sido declarada");

                return new NodoExpresion(nombre);
            }

            // Subexpresión entre paréntesis: ( Expresion )
            if (token == "(")
            {
                Avanzar();
                NodoExpresion inner = Expresion();

                if (token != ")")
                    Error("Falta ')' de cierre en la expresión");
                else
                    Avanzar();

                return new NodoExpresion("()", inner, null);
            }

            Error($"Se esperaba un operando, se encontró '{token}'");
            return new NodoExpresion("?");
        }

        // ── Expresion ────────────────────────────────────────────────────────
        // Gramática plana del pizarrón:
        //   Expresion → Operando { Operador Operando }
        //             | ( Expresion )
        private NodoExpresion Expresion()
        {
            NodoExpresion izq = Operando();

            while (EsOperador())
            {
                string op = token;
                Avanzar();

                NodoExpresion der = Operando();
                izq = new NodoExpresion(op, izq, der);
            }

            return izq;
        }

        // ── AnalizarExpresion ────────────────────────────────────────────────
        // Punto de entrada: analiza la expresión y muestra su árbol
        private NodoExpresion AnalizarExpresion()
        {
            if (!EsInicioOperando())
            {
                Error($"Se esperaba una expresión, se encontró '{token}'");
                return new NodoExpresion("?");
            }

            NodoExpresion raiz = Expresion();
            ImprimirArbol(raiz);
            return raiz;
        }

        // ════════════════════════════════════════════════════════════════════
        //  LÉXICO
        // ════════════════════════════════════════════════════════════════════

        private char Tipo_caracter(int caracter)
        {
            if ((caracter >= 65 && caracter <= 90) || (caracter >= 97 && caracter <= 122)) return 'l';
            if (caracter >= 48 && caracter <= 57) return 'd';

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

        private void Simbolo()
        {
            char ch = (char)i_caracter;
            elemento = ch.ToString() + "\n";

            bool esValido =
                i_caracter == '!' ||
                (i_caracter >= 35 && i_caracter <= 38) ||
                (i_caracter >= 40 && i_caracter <= 45) ||
                i_caracter == 47 ||
                (i_caracter >= 58 && i_caracter <= 62) ||
                i_caracter == 91 || i_caracter == 93 ||
                i_caracter == 94 || i_caracter == 123 ||
                i_caracter == 124 || i_caracter == 125 ||
                i_caracter == ',' || i_caracter == ';' ||
                i_caracter == '<' || i_caracter == '>' || i_caracter == '=';

            if (!esValido)
                Error($"Símbolo inesperado '{ch}'");
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

        private void Archivo_Libreria()
        {
            i_caracter = Leer.Read();
            if ((char)i_caracter == 'h')
            {
                Escribir.Write("libreria\n");
                i_caracter = Leer.Read();
            }
            else
            {
                Error(i_caracter);
            }
        }

        private bool Palabra_Reservada()
        {
            return P_Reservadas.IndexOf(elemento.ToLower()) >= 0;
        }

        private void Identificador()
        {
            elemento = "";
            do
            {
                elemento += (char)i_caracter;
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'l' || Tipo_caracter(i_caracter) == 'd');

            if ((char)i_caracter == '.')
            {
                Archivo_Libreria();
            }
            else if (Palabra_Reservada())
            {
                Escribir.Write(elemento.ToLower() + "\n");
            }
            else
            {
                Escribir.Write("identificador\n");
                Escribir.Write(elemento + "\n");
            }
        }

        private void Numero_Real()
        {
            do { i_caracter = Leer.Read(); } while (Tipo_caracter(i_caracter) == 'd');
            Escribir.Write("numero_real\n");
        }

        private void Numero()
        {
            do { i_caracter = Leer.Read(); } while (Tipo_caracter(i_caracter) == 'd');

            if ((char)i_caracter == '.')
                Numero_Real();
            else
                Escribir.Write("numero\n");
        }

        private bool Comentario()
        {
            // Peek ahead: if next char is / or *, it's a comment
            int siguiente = Leer.Peek();

            if (siguiente == 47) // second '/'
            {
                Leer.Read(); // consume it
                do { i_caracter = Leer.Read(); } while (i_caracter != 10 && i_caracter != -1);
                return true;
            }
            else if (siguiente == 42) // '*'
            {
                Leer.Read(); // consume '*'
                do
                {
                    do
                    {
                        i_caracter = Leer.Read();
                        if (i_caracter == 10) Numero_linea++;
                    } while (i_caracter != 42 && i_caracter != -1);

                    i_caracter = Leer.Read();
                } while (i_caracter != 47 && i_caracter != -1);

                if (i_caracter == -1) Error(i_caracter);
                i_caracter = Leer.Read();
                return true;
            }
            else
            {
                // Solo '/' sin segundo carácter: es división
                return false;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  SINTÁCTICO — infraestructura
        // ════════════════════════════════════════════════════════════════════

        private void Avanzar()
        {
            token = Leer.ReadLine()?.Trim();

            if (token == "LF")
            {
                Numero_linea++;
                Avanzar();
                return;
            }

            if (token == "identificador")
                valorToken = Leer.ReadLine()?.Trim();
        }

        private void AnalizadorSintactico()
        {
            Numero_linea = 1;
            Leer = new StreamReader(archivoback);

            TablaFunciones.Clear();
            PilaAmbitos.Clear();
            direccionActual = 0;

            TablaFunciones.Add(new SimboloFuncion("printf", "int"));
            EntrarAmbito();

            Avanzar();
            Cabecera();
            Leer.Close();
        }

        private void Cabecera()
        {
            while (token != null && token != "Fin")
            {
                switch (token)
                {
                    case "#":
                        Avanzar();
                        if (token == "include" || token == "define")
                            Directiva_proc();
                        else
                        {
                            Error("Error en directiva");
                            Avanzar();
                        }
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

        private int Directiva_proc()
        {
            if (token == "include")
            {
                Avanzar();

                if (token == "<")
                {
                    Avanzar();

                    if (token == ">")
                    {
                        Error("Se esperaba el nombre de la librería entre '<' y '>'");
                        Avanzar();
                        return 0;
                    }

                    if (token == "Fin" || token == "LF")
                    {
                        Error("Falta '>' al final del include");
                        return 0;
                    }

                    // Consumir contenido hasta '>'
                    bool tieneLibreria = false;
                    while (token != ">" && token != "Fin" && token != "LF")
                    {
                        if (token == "libreria") tieneLibreria = true;
                        Avanzar();
                    }

                    if (token != ">")
                    {
                        Error("Falta '>' al final del include");
                        return 0;
                    }

                    if (!tieneLibreria)
                        Error("No se encontró una librería válida en el include");

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
            tipoActual = token;
            Avanzar();

            if (token == null) { Error("Declaración incompleta"); return; }
            if (token == "main") { Procesar_Main(); return; }

            if (token != "identificador")
            {
                Error("Se esperaba un identificador después del tipo de dato");
                Avanzar();
                return;
            }

            string nombreVariable = valorToken;
            Avanzar();

            if (token == null) { Error("Declaración incompleta"); return; }

            switch (token)
            {
                case ";":
                    if (ExisteVariableEnAmbitoActual(nombreVariable))
                        Error($"La variable '{nombreVariable}' ya fue declarada en este ámbito");
                    else
                        AgregarVariable(nombreVariable);
                    Avanzar();
                    break;

                case "=":
                    if (ExisteVariableEnAmbitoActual(nombreVariable))
                        Error($"La variable '{nombreVariable}' ya fue declarada en este ámbito");
                    else
                        AgregarVariable(nombreVariable);
                    Dec_VGlobal();
                    break;

                case "[":
                    if (ExisteVariableEnAmbitoActual(nombreVariable))
                        Error($"La variable '{nombreVariable}' ya fue declarada en este ámbito");
                    else
                        AgregarVariable(nombreVariable);
                    D_Arreglos();
                    break;

                case "(":
                    Definicion_Funcion(nombreVariable);
                    break;

                default:
                    Error($"Se esperaba ';', '=', '[' o '(' pero se encontró '{token}'");
                    Avanzar();
                    break;
            }
        }

        // Dec_VGlobal: ahora usa AnalizarExpresion en lugar del loop manual
        private void Dec_VGlobal()
        {
            Avanzar();

            if (token == null) { Error("Inicialización incompleta después de '='"); return; }

            if (EsInicioOperando())
            {
                AnalizarExpresion();

                while (token == "LF") Avanzar();

                if (token == ";")
                    Avanzar();
                else
                    Error(token, ";");
            }
            else if (token == "[")
            {
                D_Arreglos();
            }
            else if (token == ";")
            {
                Error("Falta expresión después de '=' en la inicialización");
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

        private int Constante()
        {
            switch (token)
            {
                case "-":
                    token = Leer.ReadLine();
                    return (token == "numero_real" || token == "numero" || token == "identificador") ? 1 : 0;
                case "numero_real":
                case "numero":
                case "caracter":
                case "identificador":
                    return 1;
                default:
                    return 0;
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

                if (token != "]") { Error("Falta ']'"); return; }
                Avanzar();

                if (token == "[") { Avanzar(); continue; }
                break;
            }

            if (token == "=")
            {
                Avanzar();
                if (token != "{") { Error("Se esperaba '{' para iniciar la inicialización"); return; }

                int balance = 1;
                bool primerElemento = true;
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
                    else if (token == "numero" || token == "identificador" ||
                             token == "numero_real" || token == "caracter")
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
                Error("Falta ';' al final del arreglo");
            else
                Avanzar();
        }

        private void Bloque_Codigo(bool crearAmbito = true)
        {
            if (crearAmbito) EntrarAmbito();
            Avanzar();

            while (token != "}" && token != "Fin" && token != null)
            {
                switch (token)
                {
                    case ";":
                        Avanzar();
                        break;

                    case "=":
                        Error("Asignación inválida: falta variable del lado izquierdo");
                        Avanzar();
                        break;

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

                    case "return":
                        Avanzar();
                        if (EsInicioOperando())
                            AnalizarExpresion();
                        if (token == ";") Avanzar();
                        else Error("Falta ';' después del return");
                        break;

                    case "++":
                    case "--":
                        // Operador prefijo como sentencia: ++x; o --x;
                        AnalizarExpresion();
                        if (token == ";") Avanzar();
                        break;

                    case "identificador":
                        string nombreUso = valorToken;
                        Avanzar();

                        if (token == "=")
                        {
                            // Asignación: variable = Expresion ;
                            if (!VariableDeclarada(nombreUso))
                                Error($"La variable '{nombreUso}' no ha sido declarada");

                            Avanzar();

                            if (EsInicioOperando())
                                AnalizarExpresion();
                            else
                                Error("Se esperaba una expresión después de '='");

                            if (token == ";")
                                Avanzar();
                            else
                                Error("Falta ';' al final de la asignación");

                            continue;
                        }
                        else if (token == "(")
                        {
                            // Llamada a función como sentencia
                            if (!ExisteFuncion(nombreUso))
                                Error($"La función '{nombreUso}' no ha sido declarada");

                            NodoExpresion nodoLlamada = new NodoExpresion($"func:{nombreUso}");
                            Avanzar();

                            if (token != ")")
                            {
                                NodoExpresion primerArg = Expresion();
                                nodoLlamada.Izquierdo = primerArg;
                                NodoExpresion actual = nodoLlamada;

                                while (token == ",")
                                {
                                    Avanzar();
                                    NodoExpresion argExtra = Expresion();
                                    actual.Derecho = new NodoExpresion(",", argExtra, null);
                                    actual = actual.Derecho;
                                }
                            }

                            if (token == ")")
                                Avanzar();
                            else
                                Error("Falta ')' al cerrar la llamada a función");

                            if (token == ";") Avanzar();
                        }
                        else if (token == "++" || token == "--")
                        {
                            // Postfijo como sentencia: a++;  a--;
                            if (!VariableDeclarada(nombreUso))
                                Error($"La variable '{nombreUso}' no ha sido declarada");

                            ImprimirArbol(new NodoExpresion(nombreUso + token));
                            Avanzar();

                            if (token == ";") Avanzar();
                        }
                        else
                        {
                            // Uso de variable en expresión sin asignación
                            if (!VariableDeclarada(nombreUso))
                                Error($"La variable '{nombreUso}' no ha sido declarada");

                            while (token != ";" && token != "Fin")
                            {
                                if (token == "identificador")
                                {
                                    string subUso = valorToken;
                                    if (!VariableDeclarada(subUso))
                                        Error($"La variable '{subUso}' no ha sido declarada");
                                }
                                Avanzar();
                            }

                            if (token == ";") Avanzar();
                        }
                        break;

                    case "LF":
                        Avanzar();
                        break;

                    default:
                        if (token != "LF" && token != "Fin")
                        {
                            if (token == "+" || token == "-" || token == "*" || token == "/")
                                Avanzar();
                            else
                            {
                                Error("Token no reconocido en bloque: " + token);
                                Avanzar();
                            }
                        }
                        break;
                }
            }

            if (token == "}")
            {
                if (crearAmbito) SalirAmbito();
                Avanzar();
            }
            else if (token == "Fin")
            {
                Error("Falta la llave de cierre '}' al final del bloque");
            }
        }

        // Validar_Expresion_Parentesis: ahora usa Expresion() en lugar del loop con contador
        private void Validar_Expresion_Parentesis()
        {
            Avanzar(); // consumir el '(' ya verificado por quien llama

            if (EsInicioOperando())
            {
                NodoExpresion nodo = Expresion();
                ImprimirArbol(nodo);
            }

            if (token != ")")
                Error("Paréntesis no balanceados en la expresión");
            else
                Avanzar();
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
                    Bloque_Codigo();
                else if (token == "if")
                    Estructura_If();
                else
                    Error("Se esperaba '{' o 'if' después de 'else'");
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

                    while (token != "break" && token != "case" && token != "default" &&
                           token != "}" && token != "Fin")
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

        private void Procesar_Main()
        {
            Avanzar();

            if (token != "(")
                Error("Falta '(' después de main");
            else
                Avanzar();

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
                Bloque_Codigo();
            else
                Error("Falta '{' para iniciar el cuerpo del main");
        }

        private void Definicion_Funcion(string nombreFuncion)
        {
            if (ExisteFuncion(nombreFuncion))
                Error($"La función '{nombreFuncion}' ya fue declarada");

            SimboloFuncion funcion = new SimboloFuncion(nombreFuncion, tipoActual);
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

                    string tipoParametro = token;
                    funcion.TiposParametros.Add(tipoParametro);
                    funcion.NumeroParametros++;
                    Avanzar();

                    if (token != "identificador")
                    {
                        Error("Se esperaba un identificador como nombre del parámetro");
                        return;
                    }

                    string nombreParametro = valorToken;

                    if (ExisteVariableEnAmbitoActual(nombreParametro))
                    {
                        Error($"El parámetro '{nombreParametro}' ya fue declarado");
                    }
                    else
                    {
                        PilaAmbitos.Peek().Add(new SimboloVariable(nombreParametro, tipoParametro, direccionActual));
                        direccionActual += 4;
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
                TablaFunciones.Add(funcion);
                Bloque_Codigo();
            }
            else
            {
                Error("Se esperaba '{' para iniciar el cuerpo de la función");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  EVENTOS DE INTERFAZ
        // ════════════════════════════════════════════════════════════════════

        private void analizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "";
            guardar();

            elemento = "";
            N_error = 0;
            Numero_linea = 1;

            if (archivo == null) { MessageBox.Show("No hay archivo cargado."); return; }

            archivoback = archivo.Remove(archivo.Length - 1) + "back";
            Escribir = new StreamWriter(archivoback);
            Leer = new StreamReader(archivo);

            i_caracter = Leer.Read();

            do
            {
                elemento = "";

                switch (Tipo_caracter(i_caracter))
                {
                    case 'l': Identificador(); break;
                    case 'd': Numero(); break;
                    case 's':
                        // Operadores de dos caracteres: == != <= >= ++ -- && ||
                        if (i_caracter == '=' || i_caracter == '!' ||
                            i_caracter == '<' || i_caracter == '>' ||
                            i_caracter == '+' || i_caracter == '-' ||
                            i_caracter == '&' || i_caracter == '|')
                        {
                            int primero = i_caracter;
                            int segundo = Leer.Peek();

                            if ((primero == '=' && segundo == '=') ||
                                (primero == '!' && segundo == '=') ||
                                (primero == '<' && segundo == '=') ||
                                (primero == '>' && segundo == '=') ||
                                (primero == '+' && segundo == '+') ||
                                (primero == '-' && segundo == '-') ||
                                (primero == '&' && segundo == '&') ||
                                (primero == '|' && segundo == '|'))
                            {
                                Leer.Read(); // consumir segundo carácter
                                string doble = ((char)primero).ToString() + ((char)segundo).ToString();
                                Escribir.Write(doble + "\n");
                                i_caracter = Leer.Read();
                            }
                            else
                            {
                                Simbolo();
                                Escribir.Write(elemento);
                                i_caracter = Leer.Read();
                            }
                        }
                        else
                        {
                            Simbolo();
                            Escribir.Write(elemento);
                            i_caracter = Leer.Read();
                        }
                        break;
                    case '/':
                        // Puede ser comentario (//) o bloque (/* */) o división
                        if (Comentario())
                        {
                            Escribir.Write("Comentario\n");
                            // i_caracter apunta al char después del comentario, el loop lo procesa
                        }
                        else
                        {
                            // Es división simple: escribir '/' y avanzar
                            Escribir.Write("/\n");
                            i_caracter = Leer.Read();
                        }
                        break;
                    case '"':
                        Cadena();
                        Escribir.Write("Cadena\n");
                        i_caracter = Leer.Read();
                        break;
                    case 'c':
                        Caracter();
                        Escribir.Write("Caracter\n");
                        i_caracter = Leer.Read();
                        break;
                    case 'n':
                        i_caracter = Leer.Read();
                        Numero_linea++;
                        Escribir.Write("LF\n");
                        break;
                    case 'e':
                        i_caracter = Leer.Read();
                        break;
                    default:
                        // Punto al inicio: .6 → numero_real válido en C
                        if (i_caracter == '.')
                        {
                            int siguiente = Leer.Peek();
                            if (siguiente >= '0' && siguiente <= '9')
                            {
                                // leer los dígitos decimales
                                do { i_caracter = Leer.Read(); } while (Tipo_caracter(i_caracter) == 'd');
                                Escribir.Write("numero_real\n");
                            }
                            else
                            {
                                Error($"Símbolo inesperado '.'");
                                i_caracter = Leer.Read();
                            }
                        }
                        else
                        {
                            Error(i_caracter);
                        }
                        break;
                }
            } while (i_caracter != -1);

            Escribir.Write("Fin\n");
            richTextBox2.AppendText("Errores: " + N_error + "\n");
            Escribir.Close();
            Leer.Close();
            AnalizadorSintactico();
        }

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
            if (archivo != null)
            {
                using (StreamWriter EscribirF = new StreamWriter(archivo))
                {
                    EscribirF.Write(richTextBox1.Text);
                }
            }
            else
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
            }
            this.Text = "Mi Compilador - " + archivo;
        }

        private void guardarToolStripMenuItem_Click(object sender, EventArgs e) => guardar();

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
                Application.Exit();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            analizarToolStripMenuItem.Enabled = true;
        }

        private void Error(int i_caracter)
        {
            richTextBox2.AppendText($"Error léxico {(char)i_caracter}, línea {Numero_linea}\n");
            N_error++;
            i_caracter = Leer.Read();
        }

        private void Error(string mensaje)
        {
            richTextBox2.AppendText($"Error: {mensaje}, línea {Numero_linea}\n");
            N_error++;
        }

        private void Error(string tokenLocal, string esperado)
        {
            richTextBox2.AppendText($"Error: se esperaba '{esperado}', pero se encontró '{tokenLocal}', línea {Numero_linea}\n");
            N_error++;
        }
    }
}