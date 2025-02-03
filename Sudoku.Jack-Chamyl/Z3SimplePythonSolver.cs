using System;
using Python.Runtime;
using Sudoku.Shared;

namespace Sudoku.Z3Solvers
{
    /// <summary>
    /// Solveur Sudoku utilisant un script Python (z3solver.py) via Python.NET.
    /// </summary>
    public class Z3SimplePythonSolver : PythonSolverBase
    {
        public override SudokuGrid Solve(SudokuGrid s)
        {
            // Obtenir le GIL afin d'interagir avec l'interpréteur Python.
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
                            // Conversion de chaque cellule en objet Python (int).
                            row.Append(s.Cells[i, j].ToPython());
                        }
                        pyCells.Append(row);
                    }

                    // Définir la variable "instance" dans le scope Python.
                    scope.Set("instance", pyCells);

                    // Lire et exécuter le script Python depuis le fichier "z3solver.py".
                    string code = GetPythonScript();
                    scope.Exec(code);

                    // Récupérer la fonction "solve_sudoku" définie dans le script.
                    dynamic solveFunc = scope.Get("solve_sudoku");
                    // Appeler la fonction en lui passant la grille convertie.
                    dynamic solutionTuple = solveFunc(pyCells);

                    // On s'attend à recevoir un tuple (status, solution)
                    string status = solutionTuple[0].ToString();

                    if (status == "sat")
                    {
                        dynamic pySolution = solutionTuple[1];
                        var managedResult = new int[9, 9];

                        // Conversion de la solution Python (liste de listes) vers un tableau 2D C#.
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
                        Console.WriteLine("Aucune solution trouvée.");
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
            return System.IO.File.ReadAllText("z3solver.py");
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