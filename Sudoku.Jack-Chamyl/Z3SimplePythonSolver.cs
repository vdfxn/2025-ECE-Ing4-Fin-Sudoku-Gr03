using System;
using Python.Runtime;
using Sudoku.Shared; // Assurez-vous que SudokuGrid est défini (par exemple, avec une propriété int[,] Cells)

namespace Sudoku.Z3Solvers
{
    /// <summary>
    /// Solveur Sudoku utilisant un script Python (z3solver.py) via Python.NET.
    /// Cette version applique la méthode de substitution pour fixer les valeurs initiales.
    /// </summary>
    public class Z3SimplePythonSolver : PythonSolverBase
    {
        public override SudokuGrid Solve(SudokuGrid s)
        {
            // Obtenir le GIL pour interagir avec l'interpréteur Python.
            using (Py.GIL())
            {
                // Créer un scope Python pour exécuter le script.
                using (PyModule scope = Py.CreateScope())
                {
                    // Conversion de la grille C# (SudokuGrid) en une liste Python de listes.
                    var pyCells = new PyList();
                    for (int i = 0; i < 9; i++)
                    {
                        var row = new PyList();
                        for (int j = 0; j < 9; j++)
                        {
                            // Utiliser explicitement PyInt pour convertir chaque entier.
                            int val = s.Cells[i, j];
                            row.Append(new PyInt(val));
                        }
                        pyCells.Append(row);
                    }
                    scope.Set("instance", pyCells);

                    // Lire et exécuter le script Python depuis le fichier "z3solver.py".
                    string code = System.IO.File.ReadAllText("z3solver_advanced.py");
                    scope.Exec(code);

                    // Récupérer la fonction "solve_sudoku" définie dans le script Python.
                    dynamic solveFunc = scope.Get("solve_sudoku");
                    // Appeler la fonction en lui passant la grille convertie.
                    dynamic solutionTuple = solveFunc(pyCells);

                    // On s'attend à recevoir un tuple (status, solution)
                    string status = solutionTuple[0].ToString();

                    if (status == "sat")
                    {
                        dynamic pySolution = solutionTuple[1];
                        var managedResult = new int[9, 9];

                        // Conversion de la solution Python (liste de listes) en tableau 2D C#.
                        for (int i = 0; i < 9; i++)
                        {
                            for (int j = 0; j < 9; j++)
                            {
                                managedResult[i, j] = pySolution[i][j].As<int>();
                            }
                        }
                        return new SudokuGrid { Cells = managedResult };
                    }
                    else
                    {
                        Console.WriteLine("Aucune solution trouvée : " + solutionTuple[1].ToString());
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Lit le contenu du script Python depuis le fichier "z3solver.py".
        /// </summary>
        /// <returns>Le contenu du fichier sous forme de chaîne.</returns>
        protected virtual string GetPythonScript()
        {
            return System.IO.File.ReadAllText("z3solver_advanced.py");
        }

        /// <summary>
        /// Initialise les modules Python nécessaires via pip.
        /// </summary>
        protected override void InitializePythonComponents()
        {
            InstallPipModule("numpy");
            InstallPipModule("z3-solver==4.12.2.0");
            base.InitializePythonComponents();
        }
    }
}