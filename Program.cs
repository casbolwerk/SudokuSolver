using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace Try
{
    class Program
    {
        static Stopwatch time = new Stopwatch();

        static void Main(string[] args)
        {
            // Check if command contains an algorithm name we should use
            // and the txt file we should read
            string algorithm = "CB";
            if (args.Contains("-CB"))
            {
                algorithm = "CB";
            }
            else if (args.Contains("-FC"))
            {
                algorithm = "FC";
            }

            string txtFile = "";
            if (args.Contains("-sud"))
            {
                int sudInd = Array.IndexOf(args, "-sud");
                txtFile = args[sudInd + 1];
            }
            else
            {
                Console.WriteLine("A .txt file should be given, after -sud");
            }

            // Get path of txt file
            string workingDirectory = Environment.CurrentDirectory;
            string txtPath = workingDirectory + "/" + txtFile;
            
            // Read a text file line by line.  
            string[] lines = File.ReadAllLines(txtPath);
            int lineInd = 0;
            int numSudoku = 0;
            List<Knoop> sudokus = new List<Knoop>();
            List<int> ns = new List<int>();
            while (lineInd < lines.Length)
            {
                // If the line starts with a letter,
                // that's an indication that the next N lines
                // will be a sudoku
                if (lines[lineInd].Any(x => char.IsLetter(x)))
                {
                    lineInd++;
                    numSudoku++;

                    char[] chars = lines[lineInd].ToCharArray();
                    string[] line = Array.ConvertAll(chars, char.ToString);
                    int[] intarray = Array.ConvertAll<string, int>(line, int.Parse);
                    int N = line.Length;
                    ns.Add(N);

                    //N x N 2D array is generated
                    //values in the first line are added to the array
                    //& checked whether fixed values.
                    int[] inputState = new int[N*N];

                    for (int j = 0; j < N; j++)
                    {
                        inputState[j] = intarray[j];
                    }

                    for (int nextLines = 1; nextLines < N; nextLines++)
                    {
                        chars = lines[lineInd + nextLines].ToCharArray();
                        line = Array.ConvertAll(chars, char.ToString);
                        intarray = Array.ConvertAll<string, int>(line, int.Parse);

                        for (int nextVal = nextLines*N; nextVal < (nextLines+1)*N; nextVal++)
                        {
                            inputState[nextVal] = intarray[nextVal-nextLines*N];
                        }
                    }
                    lineInd += N;

                    int wortelN = (int)Math.Sqrt(N);
                    List<int>[] kolommen = MaakKolommen(N, inputState);
                    List<int>[] rijen = MaakRijen(N, inputState);
                    List<int>[] blokken = MaakBlokken(N, wortelN, inputState);              // blokken rijen en kolommen worden aangemaakt van de sudoku
                    List<int>[] domein = MaakDomeinen(N, wortelN, inputState, kolommen, rijen, blokken);

                    Knoop input = new Knoop(inputState, kolommen, rijen, blokken, domein);
                    sudokus.Add(input);
                }
                else
                {
                    lineInd++;
                }
            }


            // Now that the sudoku has been read and the
            // algorithm determined, run the algorithm
            // on the sudoku
            List<int> evalScores = new List<int>();
            for (int tempNum = 0; tempNum < numSudoku; tempNum++)
            {
                bool resultaat = false;
                Knoop input = sudokus[tempNum];
                int N = ns[tempNum];
                int worteln = (int)Math.Sqrt(N);

                // If the algorithm is Chronological
                // Backtracking, use that on the sudokus
                // that are to be solved
                if (algorithm == "CB")
                {
                    time.Start();
                    resultaat = BackTracking(input, 0, N, worteln);
                    time.Stop();
                    if (resultaat)
                    {
                        // Some output that is printed when a result has been found
                        Console.WriteLine("Time elapsed for CB: {0}", time.Elapsed);
                        time.Reset();
                        Console.WriteLine("Amount of nodes expanded: " + input.i);
                        int[] output = input.sudoku;
                        for (int ib = 0; ib < N * N; ib++)
                        {
                            Console.Write(output[ib]);
                            if ((ib + 1) % 9 == 0)
                            {
                                Console.Write("\n");
                            }
                        }
                    }
                    else
                    {
                        // Because CB is complete, when it doesn't have an outcome,
                        // it means there isn't a solution to the sudoku
                        Console.WriteLine("There is no solution to this sudoku.");
                        time.Reset();
                    }
                }
                // If the algorithm is Forward
                // Checking, use that on the sudokus
                // that are to be solved
                else if (algorithm == "FC")
                {
                    time.Start();
                    resultaat = StandardForwardChecking(input, 0, N, worteln);
                    time.Stop();
                    if (resultaat)
                    {
                        // Some output that is printed when a result has been found
                        Console.WriteLine("Time elapsed for FC: {0}", time.Elapsed);
                        time.Reset();
                        Console.WriteLine("Amount of nodes expanded: " + input.i);
                        int[] output = input.sudoku;
                        for (int ib = 0; ib < N * N; ib++)
                        {
                            Console.Write(output[ib]);
                            if ((ib + 1) % 9 == 0)
                            {
                                Console.Write("\n");
                            }
                        }
                    }
                    else
                    {
                        // Because FC is complete, when it doesn't have an outcome,
                        // it means there isn't a solution to the sudoku
                        Console.WriteLine("There is no solution to this sudoku.");
                        time.Reset();
                    }
                }

                Console.ReadLine();
            }

            Console.ReadLine();
        }
        // ALGORITMES //

        static bool BackTracking(Knoop k, int p, int n, int worteln)           // Knoop k is beginknoop, p staat voor positie die we invullen  
        {                                                                       // en n is voor grid grootte, worteln wortel van n

            if (p == n * n)                                                     // als we bij de laatste positie zijn aangekomen dan zijn er geen schendingen
                return true;                                                    // en hebben we een oplossing
            k.i++;


            if (k.sudoku[p] == 0)
            {
                (int x, int y) = VraagCoOrdinaat(n, p);                         // het recursieve gedeelte
                int z = (x / worteln) + (y / worteln) * worteln;                // x: kolomnummer, y: rijnummer, z: bloknummer

                for (int toewijzing = 1; toewijzing <= n; toewijzing++)         // toewijzing van een invulling aan positie p
                {
                    if (!k.kolom[x].Contains(toewijzing)
                        && !k.rij[y].Contains(toewijzing)
                        && !k.blok[z].Contains(toewijzing))                     // er wordt gecheckt of op die plek dit cijfer kan
                    {
                        k.sudoku[p] = toewijzing; k.kolom[x].Add(toewijzing);   //pas knoop aan door aan alle delen de nieuwe toewijzing
                        k.rij[y].Add(toewijzing); k.blok[z].Add(toewijzing);    //toe te voegen


                        if (BackTracking(k, p + 1, n, worteln))
                        return true;                              // als er niks wordt geschonden, backtrack dan verder

                        k.sudoku[p] = 0; k.kolom[x].Remove(toewijzing);
                        k.rij[y].Remove(toewijzing); k.blok[z].Remove(toewijzing);
                    }
                }
            }
            else
                return BackTracking(k, p + 1, n, worteln);

            return false;
        }


        static bool StandardForwardChecking(Knoop k, int p, int n, int worteln)
        {

            if (p == n * n)                                                     // als we bij de laatste positie zijn aangekomen dan zijn er geen schendingen
                return true;                                                    // en hebben we een oplossing
            k.i++;

            if (k.sudoku[p] == 0)
            {
                (int x, int y) = VraagCoOrdinaat(n, p);                         // het recursieve gedeelte
                int z = (x / worteln) + (y / worteln) * worteln;                // x: kolomnummer, y: rijnummer, z: bloknummer

                for (int getal = 1; getal <= n; getal++)
                {
                    if (k.domein[p].Contains(getal))
                    {
                        // er worden lijsten aangemaakt die bij zullen houden van welke
                        // vakjes er in het domein iets verwijderd wordt
                        List<int> verwijderdRij = new List<int>();
                        List<int> verwijderdKolom = new List<int>();
                        List<int> verwijderdBlok = new List<int>();

                        // het vakje wordt ingevuld met een getal
                        k.sudoku[p] = getal;

                        // de vakjes met dezelfde x worden gecheckt op domein,
                        // als getal in het domein zit, wordt deze eruit gehaald
                        bool kolomFout = false;
                        for (int ko = x; ko < ((n - 1) * n + x) + 1; ko += n)       //kolom
                        {
                            if (k.sudoku[ko] == 0 && k.domein[ko].Contains(getal))
                            {
                                k.domein[ko].Remove(getal);

                                // als er na verwijdering niks meer in het domein van een vakje
                                // zit, geeft het programma een foutmelding en is het huidige getal fout
                                if (!k.domein[ko].Any())
                                {
                                    k.domein[ko].Add(getal);

                                    List<int>[] kolomFoutArray = DomeinToevoeging(k, getal, verwijderdKolom, verwijderdRij, verwijderdBlok);

                                    for (int i = 0; i < kolomFoutArray.Length; i++)
                                    {
                                        k.domein[i] = kolomFoutArray[i];
                                    }

                                    k.sudoku[p] = 0;

                                    kolomFout = true;
                                    break;
                                }

                                // anders wordt het getal uit het domein gehaald
                                // en wordt de coordinaat van het vakje opgeslagen
                                verwijderdKolom.Add(ko);
                            }


                        }
                        if (kolomFout == true)
                        {
                            kolomFout = false;
                            continue;
                        }

                        // de vakjes met dezelfde y worden gecheckt op domein,
                        // als getal in het domein zit, wordt deze eruit gehaald
                        bool rijFout = false;
                        for (int r = y * n; r < (y * n + n); r++)                   //rij
                        {
                            if (k.sudoku[r] == 0 && k.domein[r].Contains(getal))
                            {
                                k.domein[r].Remove(getal);

                                // als er na verwijdering niks meer in het domein van een vakje
                                // zit, geeft het programma een foutmelding en is het huidige getal fout
                                if (!k.domein[r].Any())
                                {
                                    k.domein[r].Add(getal);

                                    List<int>[] rijFoutArray = DomeinToevoeging(k, getal, verwijderdKolom, verwijderdRij, verwijderdBlok);

                                    for (int i = 0; i < rijFoutArray.Length; i++)
                                    {
                                        k.domein[i] = rijFoutArray[i];
                                    }

                                    k.sudoku[p] = 0;

                                    rijFout = true;
                                    break;
                                }

                                // anders wordt het getal uit het domein gehaald
                                // en wordt de coordinaat van het vakje opgeslagen
                                verwijderdRij.Add(r);
                            }

                        }
                        if (rijFout == true)
                        {
                            rijFout = false;
                            continue;
                        }

                        // de vakjes met hetzelfde bloknummer worden gecheckt op domein,
                        // als getal in het domein zit, wordt deze eruit gehaald
                        bool blokFout = false;
                        for (int b = 0; b < n * n; b++)                              //blok
                        {
                            (int X, int Y) = VraagCoOrdinaat(n, b);
                            int Z = (X / worteln) + (Y / worteln) * worteln;

                            if (z == Z)
                            {
                                if (k.sudoku[b] == 0 && k.domein[b].Contains(getal))
                                {
                                    k.domein[b].Remove(getal);

                                    // als er na verwijdering niks meer in het domein van een vakje
                                    // zit, geeft het programma een foutmelding en is het huidige getal fout
                                    if (!k.domein[b].Any())
                                    {
                                        k.domein[b].Add(getal);

                                        List<int>[] blokFoutArray = DomeinToevoeging(k, getal, verwijderdKolom, verwijderdRij, verwijderdBlok);

                                        for (int i = 0; i < blokFoutArray.Length; i++)
                                        {
                                            k.domein[i] = blokFoutArray[i];
                                        }

                                        k.sudoku[p] = 0;

                                        blokFout = true;
                                        break;
                                    }

                                    // anders wordt het getal uit het domein gehaald
                                    // en wordt de coordinaat van het vakje opgeslagen
                                    verwijderdBlok.Add(b);
                                }
                            }
                        }
                        if (blokFout == true)
                        {
                            blokFout = false;
                            continue;
                        }

                        // als het algoritme met het huidige getal tot een oplossing
                        // komt, is het goed ingevuld
                        if (StandardForwardChecking(k, p + 1, n, worteln))
                            return true;

                        // anders wordt het getal en alle domeinaanpassingen gereset
                        List<int>[] tempArray = DomeinToevoeging(k, getal, verwijderdKolom, verwijderdRij, verwijderdBlok);

                        for (int i = 0; i < tempArray.Length; i++)
                        {
                            k.domein[i] = tempArray[i];
                        }

                        k.sudoku[p] = 0;
                    }
                }

            }
            else
                return StandardForwardChecking(k, p + 1, n, worteln);

            return false;
        }

        

        // ALGORITMES //

        // HET AANMAKEN VAN ALLE KOLOMMEN / RIJEN / BLOKKEN VAN DE SUDOKU //
        static List<int>[] MaakKolommen(int n, int[] sudoku)
        {
            List<int>[] kolommen = new List<int>[n];

            for (int i = 0; i < n; i++)
            {
                List<int> kolom = new List<int>();

                for (int t = i; t < ((n - 1) * n + i) + 1; t += n)
                    if (sudoku[t] > 0)
                        kolom.Add(sudoku[t]);

                kolommen[i] = kolom;
            }

            return kolommen;
        }

        static List<int>[] MaakRijen(int n, int[] sudoku)
        {
            List<int>[] rijen = new List<int>[n];

            for (int i = 0; i < n; i++)
            {
                List<int> rij = new List<int>();

                for (int t = i * n; t < (i * n + n); t++)
                    if (sudoku[t] > 0)
                        rij.Add(sudoku[t]);

                rijen[i] = rij;

            }

            return rijen;
        }


        static List<int>[] MaakBlokken(int n, int worteln, int[] sudoku)
        {
            List<int>[] blokken = new List<int>[n];
            for (int i = 0; i < n; i++)
                blokken[i] = new List<int>();

            for (int p = 0; p < n * n; p++)
            {
                if (sudoku[p] > 0)
                {
                    (int x, int y) = VraagCoOrdinaat(n, p);
                    int blok = (x / worteln) + (y / worteln) * worteln;         // bereken het blok dat correspondeert met de plek in de sudoku

                    blokken[blok].Add(sudoku[p]);
                }
            }

            return blokken;

        }

        static List<int>[] MaakDomeinen(int n, int worteln, int[] sudoku, List<int>[] kolommen, List<int>[] rijen, List<int>[] blokken)
        {
            List<int>[] domein = new List<int>[n * n];

            for (int i = 0; i < n * n; i++)
            {
                if (sudoku[i] > 0)
                {
                    domein[i] = Enumerable.Range(1, n + n).ToList();                //in het domein van een reeds ingevuld vakje zit enkel het ingevulde getal
                }
                else
                    domein[i] = Enumerable.Range(1, n).ToList();                // cijers 1 t/m 9 worden toegevoegd


            }

            for (int i = 0; i < n * n; i++)
            {
                if (sudoku[i] == 0)
                {
                    (int x, int y) = VraagCoOrdinaat(n, i);                     // x: kolomnummer, y: rijnummer, z: bloknummer
                    int z = (x / worteln) + (y / worteln) * worteln;

                    foreach (int item in kolommen[x])                           //de inhoud van de corresponderende kolom,rij,blok van p wordt verwijderd uit het domein
                        domein[i].Remove(item);
                    foreach (int item in rijen[y])
                        domein[i].Remove(item);
                    foreach (int item in blokken[z])
                        domein[i].Remove(item);
                }
            }

            return domein;
        }
        // HET AANMAKEN VAN ALLE KOLOMMEN / RIJEN / BLOKKEN / DOMEINEN VAN DE SUDOKU //



        // HULPMETHODES//
        static (int, int) VraagCoOrdinaat(int n, int t)
        {
            return (t % n, t / n);
        }

        static List<int>[] DomeinToevoeging(Knoop k, int getal, List<int> verwijderdKolom, List<int> verwijderdRij, List<int> verwijderdBlok)
        {
            // zelfde idee maar dan als toevoeging, als een vakje in lijn staat met p, voeg dan getal weer toe aan het domein
            for (int kCount = 0; kCount < verwijderdKolom.Count(); kCount++)       //kolom
            {
                int ko = verwijderdKolom[kCount];

                k.domein[ko].Add(getal);
            }

            for (int rCount = 0; rCount < verwijderdRij.Count(); rCount++)                   //rij
            {
                int r = verwijderdRij[rCount];

                k.domein[r].Add(getal);

            }
            for (int bCount = 0; bCount < verwijderdBlok.Count(); bCount++)                              //blok
            {
                int b = verwijderdBlok[bCount];

                k.domein[b].Add(getal);
            }

            return k.domein;
        }
        // HULPMETHODES //
    }

    class Knoop
    {
        public int[] sudoku;
        public List<int>[] kolom;
        public List<int>[] rij;
        public List<int>[] blok;
        public List<int>[] domein;
        public int i = 0;

        public Knoop(int[] s, List<int>[] x, List<int>[] y, List<int>[] z, List<int>[] d)
        {
            sudoku = s;
            kolom = x;
            rij = y;
            blok = z;
            domein = d;
        }
    }
}