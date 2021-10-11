using System;
using System.Collections.Generic;
using System.Text;

//    Requerimiento 1: Implementar el not en el if
//    Requerimiento 2: Validar asignación de strings en Instruccion
//    Requerimiento 3: Implementar la comparación de tipos de datos en Lista_IDs
//    Requerimiento 4: Validar los tipos de datos en la asignacion del cin
//    Requerimiento 5: Implementar el cast


namespace sintaxis3
{
    class Lenguaje : Sintaxis
    {
        Stack s;
        ListaVariables l;
        Variable.tipo maxBytes;
        public Lenguaje()
        {
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        public Lenguaje(string nombre) : base(nombre)
        {
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        // Programa -> Libreria Main
        public void Programa()
        {
            Libreria();
            Main();
            l.imprime(bitacora);
        }

        // Libreria -> (#include <identificador(.h)?> Libreria) ?
        private void Libreria()
        {
            if (getContenido() == "#")
            {
                match("#");
                match("include");
                match("<");
                match(clasificaciones.identificador);

                if (getContenido() == ".")
                {
                    match(".");
                    match("h");
                }

                match(">");

                Libreria();
            }
        }

        // Main -> tipoDato main() BloqueInstrucciones 
        private void Main()
        {
            match(clasificaciones.tipoDato);
            match("main");
            match("(");
            match(")");

            BloqueInstrucciones(true);
        }

        // BloqueInstrucciones -> { Instrucciones }
        private void BloqueInstrucciones(bool ejecuta)
        {
            match(clasificaciones.inicioBloque);

            Instrucciones(ejecuta);

            match(clasificaciones.finBloque);
        }

        // Lista_IDs -> identificador (= Expresion)? (,Lista_IDs)? 
        private void Lista_IDs(string contenidoDat, bool ejecuta)
        {
            string nombre = getContenido();
            match(clasificaciones.identificador); // Validar duplicidad

            if (!l.Existe(nombre))
            {
                switch (contenidoDat)
                {
                    case "int":
                        l.Inserta(nombre, Variable.tipo.INT);
                        break;
                    case "string":
                        l.Inserta(nombre, Variable.tipo.STRING);
                        break;
                    case "float":
                        l.Inserta(nombre, Variable.tipo.FLOAT);
                        break;
                    case "char":
                        l.Inserta(nombre, Variable.tipo.CHAR);
                        break;

                }

            }
            else
            {
                // Levantar excepción
                throw new Error(bitacora, "Error de sintaxis: Variable duplicada (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            }
            string valor = "";

            if (getClasificacion() == clasificaciones.asignacion)
            {
                match(clasificaciones.asignacion);


                if (getClasificacion() == clasificaciones.cadena)
                {
                    valor = getContenido();
                    if (contenidoDat == "string")
                    {
                        match(clasificaciones.cadena);
                    }
                    else
                    {
                        throw new Error(bitacora, "Error semantico: No se puede asignar un STRING a un (" + contenidoDat + ") " + "(" + linea + ", " + caracter + ")");
                    }
                }
                else
                {
                    //Requerimiento 3
                    Expresion();
                    maxBytes = Variable.tipo.CHAR;
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    if (tipoDatoExpresion(float.Parse(valor)) > maxBytes)
                    {
                        maxBytes = tipoDatoExpresion(float.Parse(valor));
                    }
                    if (maxBytes > l.getTipoDato(nombre))
                    {
                        throw new Error(bitacora, "Error semantico: No se puede asignar un " + maxBytes + " a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                    }
                }



            }

            if (ejecuta)
            {
                l.setValor(nombre, valor);
            }


            if (getContenido() == ",")
            {
                match(",");
                Lista_IDs(contenidoDat, ejecuta);
            }
        }

        // Variables -> tipoDato Lista_IDs; 
        private void Variables(bool ejecuta)
        {
            string contenidoDat = getContenido();
            match(clasificaciones.tipoDato);
            Lista_IDs(contenidoDat, ejecuta);
            match(clasificaciones.finSentencia);
        }

        // Instruccion -> (If | cin | cout | const | Variables | asignacion) ;
        private void Instruccion(bool ejecuta)
        {
            if (getContenido() == "do")
            {
                DoWhile(ejecuta);
            }
            else if (getContenido() == "while")
            {
                While(ejecuta);
            }
            else if (getContenido() == "for")
            {
                For(ejecuta);
            }
            else if (getContenido() == "if")
            {
                If(ejecuta);
            }
            else if (getContenido() == "cin")
            {
                // Requerimiento 4
                match("cin");
                match(clasificaciones.flujoEntrada);
                string nombre = getContenido();
                match(clasificaciones.identificador); // Validar existencia
                if (!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");


                }

                if (ejecuta)
                {
                    string valor;
                    if (l.getTipoDato(nombre) == Variable.tipo.STRING)
                    {
                        valor = Console.ReadLine();
                        
                    }
                    else
                    {
                        maxBytes = Variable.tipo.CHAR;
                        valor = Console.ReadLine();
                        float auxTry;
                        if (float.TryParse(valor, out auxTry))
                        {
                            if (tipoDatoExpresion(auxTry)> maxBytes)
                            {
                                maxBytes = tipoDatoExpresion(auxTry);
                                
                            }
                            if (maxBytes > l.getTipoDato(nombre))
                            {
                                throw new Error(bitacora, "Error semantico: No se puede asignar un " + maxBytes + " a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                            }
                        }else
                        {
                            throw new Error(bitacora, "Error semantico: No se puede asignar un STRING a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                        }
                    }
                    l.setValor(nombre, valor);
                }

                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "cout")
            {
                match("cout");
                ListaFlujoSalida(ejecuta);
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "const")
            {
                Constante(ejecuta);
            }
            else if (getClasificacion() == clasificaciones.tipoDato)
            {
                Variables(ejecuta);
            }
            else
            {
                string nombre = getContenido();
                match(clasificaciones.identificador); // Validar existencia
                if (!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }

                match(clasificaciones.asignacion);

                string valor;

                //Requerimiento 2
                if (getClasificacion() == clasificaciones.cadena)
                {
                    if (l.getTipoDato(nombre) == Variable.tipo.STRING)
                    {
                        valor = getContenido();
                        match(clasificaciones.cadena);
                    }
                    else
                    {
                        throw new Error(bitacora, "Error semantico: No se puede asignar un STRING a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                    }
                }
                else
                {
                    //Requerimiento 3
                    Expresion();
                    maxBytes = Variable.tipo.CHAR;
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    if (tipoDatoExpresion(float.Parse(valor)) > maxBytes)
                    {
                        maxBytes = tipoDatoExpresion(float.Parse(valor));
                    }
                    if (maxBytes > l.getTipoDato(nombre))
                    {
                        throw new Error(bitacora, "Error semantico: No se puede asignar un " + maxBytes + " a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                    }
                }

                if (ejecuta)
                {
                    l.setValor(nombre, valor);
                }

                match(clasificaciones.finSentencia);
            }
        }

        // Instrucciones -> Instruccion Instrucciones?
        private void Instrucciones(bool ejecuta)
        {
            Instruccion(ejecuta);

            if (getClasificacion() != clasificaciones.finBloque)
            {
                Instrucciones(ejecuta);
            }
        }

        // Constante -> const tipoDato identificador = numero | cadena;
        private void Constante(bool ejecuta)
        {
            match("const");
            string contenidoDat = getContenido();
            match(clasificaciones.tipoDato);
            string nombre = getContenido();
            if (!l.Existe(nombre) && ejecuta)
            {
                match(clasificaciones.identificador); // Validar duplicidad
                switch (contenidoDat)
                {
                    case "int":
                        l.Inserta(nombre, Variable.tipo.INT, true);
                        break;
                    case "string":
                        l.Inserta(nombre, Variable.tipo.STRING, true);
                        break;
                    case "float":
                        l.Inserta(nombre, Variable.tipo.FLOAT, true);
                        break;
                    case "char":
                        l.Inserta(nombre, Variable.tipo.CHAR, true);
                        break;

                }
            }
            else
            {
                // Levantar excepción
                throw new Error(bitacora, "Error de sintaxis: Variable duplicada (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            }

            match(clasificaciones.asignacion);

            string valor;

            if (getClasificacion() == clasificaciones.cadena)
            {
                valor = getContenido();
                match(clasificaciones.cadena);
            }
            else
            {
                Expresion();
                valor = s.pop(bitacora, linea, caracter).ToString();
            }

            if (ejecuta)
            {
                l.setValor(nombre, valor);
            }


            match(clasificaciones.finSentencia);
        }

        // ListaFlujoSalida -> << cadena | identificador | numero (ListaFlujoSalida)?
        private void ListaFlujoSalida(bool ejecuta)
        {
            match(clasificaciones.flujoSalida);

            if (getClasificacion() == clasificaciones.numero)
            {
                if (ejecuta)
                {
                    Console.Write(getContenido());
                }

                match(clasificaciones.numero);
            }
            else if (getClasificacion() == clasificaciones.cadena)
            {
                string guardaCadena = getContenido();
                guardaCadena = guardaCadena.Replace("\"", "");
                if (ejecuta)
                {

                    if (guardaCadena.Contains("\\n"))
                    {
                        int inde = guardaCadena.IndexOf("\\n");
                        int varaux = 0;
                        foreach (char c in guardaCadena)
                        {
                            if (c == '\\')
                            {
                                if (varaux == inde)
                                {
                                    Console.WriteLine();
                                }
                            }
                            varaux++;
                        }
                        guardaCadena = guardaCadena.Replace("\\n", "");
                    }

                    if (guardaCadena.Contains("\\t"))
                    {
                        int inde = guardaCadena.IndexOf("\\t");
                        int varaux = 0;
                        foreach (char c in guardaCadena)
                        {
                            if (c == '\\')
                            {
                                if (varaux == inde)
                                {
                                    Console.Write("\t");
                                }
                            }
                            varaux++;
                        }
                        guardaCadena = guardaCadena.Replace("\\t", "");
                    }


                    Console.Write(guardaCadena);
                }

                match(clasificaciones.cadena);
            }
            else
            {
                string nombre = getContenido();
                if (!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }

                if (ejecuta)
                {
                    Console.Write(l.getValor(nombre));
                }
                match(clasificaciones.identificador); // Validar existencia 


            }

            if (getClasificacion() == clasificaciones.flujoSalida)
            {
                ListaFlujoSalida(ejecuta);
            }
        }

        // If -> if (Condicion) { BloqueInstrucciones } (else BloqueInstrucciones)?
        private void If(bool ejecuta2)
        {
            bool ejecuta;
            match("if");
            match("(");
            if (getContenido() == "!")
            {
                match("!");
                match("(");
                ejecuta = !Condicion();
                match(")");
            }
            else
            {
                ejecuta = Condicion();
            }
            match(")");
            BloqueInstrucciones(ejecuta && ejecuta2);

            if (getContenido() == "else")
            {
                match("else");
                BloqueInstrucciones(!ejecuta && ejecuta2);
            }
        }

        // Condicion -> Expresion operadorRelacional Expresion
        private bool Condicion()
        {
            maxBytes = Variable.tipo.CHAR;
            Expresion();
            float n1 = s.pop(bitacora, linea, caracter);
            string operador = getContenido();
            match(clasificaciones.operadorRelacional);
            maxBytes = Variable.tipo.CHAR;
            Expresion();
            float n2 = s.pop(bitacora, linea, caracter);


            switch (operador)
            {
                case ">":
                    return n1 > n2;

                case ">=":
                    return n1 >= n2;

                case "<":
                    return n1 < n2;

                case "<=":
                    return n1 <= n2;

                case "==":
                    return n1 == n2;

                default:
                    return n1 != n2;

            }
        }

        // x26 = (3+5)*8-(10-4)/2;
        // Expresion -> Termino MasTermino 
        private void Expresion()
        {
            Termino();
            MasTermino();
        }
        // MasTermino -> (operadorTermino Termino)?
        private void MasTermino()
        {
            if (getClasificacion() == clasificaciones.operadorTermino)
            {
                string operador = getContenido();
                match(clasificaciones.operadorTermino);
                Termino();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter);
                // Console.Write(operador + " ");

                switch (operador)
                {
                    case "+":
                        s.push(e2 + e1, bitacora, linea, caracter);
                        break;
                    case "-":
                        s.push(e2 - e1, bitacora, linea, caracter);
                        break;
                }

                s.display(bitacora);
            }
        }
        // Termino -> Factor PorFactor
        private void Termino()
        {
            Factor();
            PorFactor();
        }
        // PorFactor -> (operadorFactor Factor)?
        private void PorFactor()
        {
            if (getClasificacion() == clasificaciones.operadorFactor)
            {
                string operador = getContenido();
                match(clasificaciones.operadorFactor);
                Factor();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter);
                // Console.Write(operador + " ");

                switch (operador)
                {
                    case "*":
                        s.push(e2 * e1, bitacora, linea, caracter);
                        break;
                    case "/":
                        s.push(e2 / e1, bitacora, linea, caracter);
                        break;
                }

                s.display(bitacora);
            }
        }
        // Factor -> identificador | numero | ( Expresion )
        private void Factor()
        {
            if (getClasificacion() == clasificaciones.identificador)
            {


                string nombre = getContenido();

                s.display(bitacora);
                match(clasificaciones.identificador); // Validar existencia
                if (!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
                s.push(float.Parse(l.getValor(nombre)), bitacora, linea, caracter);

                if (l.getTipoDato(nombre) > maxBytes)
                {
                    maxBytes = l.getTipoDato(nombre);
                }


            }
            else if (getClasificacion() == clasificaciones.numero)
            {
                // Console.Write(getContenido() + " ");
                s.push(float.Parse(getContenido()), bitacora, linea, caracter);
                s.display(bitacora);
                if (tipoDatoExpresion(float.Parse(getContenido())) > maxBytes)
                {
                    maxBytes = tipoDatoExpresion(float.Parse(getContenido()));
                }
                match(clasificaciones.numero);
            }
            else
            {
                match("(");
                bool huboCast = false;
                Variable.tipo tipoDato = Variable.tipo.CHAR;
                if (getClasificacion() == clasificaciones.tipoDato)
                {
                    huboCast = true;
                    tipoDato = determinarTipoDato(getContenido());
                    match(clasificaciones.tipoDato);
                    match(")");
                    match("(");
                }
                Expresion();
                match(")");
                if (huboCast)
                {
                    //si hubo cast hacer un pop y convertir ese numero a tipoDato y meterlo al stack
                    float n1 = s.pop(bitacora, linea, caracter);
                    //Para convertir un entero a char necesitamos dividir entre 256 y el residuo
                    //es el resultado del cast 256=0, 257=1, 258=2, ...
                    //Para convertir un flotante a entero necesitamos dividir entre 65536 y el residuo
                    //es el resultado del cast
                    //Para convertir un flotante a otro tipo de dato, redondear el numero para eliminar
                    //la parte fraccional.
                    //Para convertir un flotante a char necesitamos dividir entre 65536/256 y el residuo
                    //es el resultado del cast
                    //Para convertir a float n1 = n1
                    //n1 = cast(n1, tipoDato);
                    n1 = cast(n1, tipoDato);
                    s.push(n1, bitacora, linea, caracter);
                    maxBytes = tipoDato;
                }
            }
        }

        // For -> for (identificador = Expresion; Condicion; identificador incrementoTermino) BloqueInstrucciones
        private void For(bool ejecuta)
        {
            match("for");

            match("(");

            string nombre = getContenido();
            if (!l.Existe(nombre))
            {
                throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            }
            Console.Write(l.getValor(nombre));
            match(clasificaciones.identificador); // Validar existencia 

            match(clasificaciones.asignacion);
            Expresion();
            match(clasificaciones.finSentencia);

            Condicion();
            match(clasificaciones.finSentencia);

            nombre = getContenido();
            if (!l.Existe(nombre))
            {
                throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            }
            Console.Write(l.getValor(nombre));
            match(clasificaciones.identificador); // Validar existencia 

            match(clasificaciones.incrementoTermino);

            match(")");

            BloqueInstrucciones(ejecuta);
        }

        // While -> while (Condicion) BloqueInstrucciones
        private void While(bool ejecuta)
        {
            match("while");

            match("(");
            Condicion();
            match(")");

            BloqueInstrucciones(ejecuta);
        }

        // DoWhile -> do BloqueInstrucciones while (Condicion);
        private void DoWhile(bool ejecuta)
        {
            match("do");

            BloqueInstrucciones(ejecuta);

            match("while");

            match("(");
            Condicion();
            match(")");
            match(clasificaciones.finSentencia);
        }

        private Variable.tipo tipoDatoExpresion(float valor)
        {
            if (valor % 1 != 0)
            {
                return Variable.tipo.FLOAT;
            }
            else if (valor < 256)
            {
                return Variable.tipo.CHAR;
            }
            else if (valor < 65535)
            {
                return Variable.tipo.INT;
            }
            return Variable.tipo.FLOAT;
        }

        private Variable.tipo determinarTipoDato(string tipoDato)
        {
            Variable.tipo tipoVar;

            switch (tipoDato)
            {
                case "int":
                    tipoVar = Variable.tipo.INT;
                    break;

                case "float":
                    tipoVar = Variable.tipo.FLOAT;
                    break;

                case "string":
                    tipoVar = Variable.tipo.STRING;
                    break;

                default:
                    tipoVar = Variable.tipo.CHAR;
                    break;
            }

            return tipoVar;
        }

        private float cast(float n1, Variable.tipo tipoDato)
        {
            if (tipoDatoExpresion(n1) == Variable.tipo.INT && tipoDato == Variable.tipo.CHAR)
            {
                n1 = n1 % 256;
            }
            else if (tipoDatoExpresion(n1) == Variable.tipo.FLOAT && tipoDato == Variable.tipo.CHAR)
            {
                n1 = (float)(Math.Round(n1));
                n1 = n1 % 65536 % 256;
            }
            else if (tipoDatoExpresion(n1) == Variable.tipo.FLOAT && tipoDato == Variable.tipo.INT)
            {
                n1 = (float)(Math.Round(n1));
                n1 = n1 % 65536;
            }
            return n1;
        }
    }
}