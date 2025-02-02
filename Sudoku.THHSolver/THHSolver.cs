using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.THHSolver
{
    public class THHSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid grid)
        {
            bool solved = false;

            while (!solved)
            {
                solved = true;
                // Répéter les étapes jusqu'à ce qu'il n'y ait plus de cases évidentes à remplir

                // Passer sur toutes les cases de la grille
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        if (grid.Cells[row, col] == 0) // Si la case est vide
                        {
                            List<int> possibleValues = GetPossibleValues(grid, row, col);

                            if (possibleValues.Count == 1)
                            {
                                // Si une seule valeur est possible, la remplir
                                grid.Cells[row, col] = possibleValues[0];
                                solved = false; // Une valeur a été remplie, continuer la boucle
                            }
                        }
                    }
                }

                // Vérifier les cases avec des candidats multiples et appliquer d'autres techniques si nécessaire
                // Par exemple, rechercher des paires cachées ou des triplets cachés.
                // Ce code peut être étendu en fonction des autres techniques humaines.
            }

            return grid;
        }

        /// <summary>
        /// Retourne les valeurs possibles pour une cellule donnée en fonction des contraintes de la ligne, colonne et sous-grille.
        /// </summary>
        /// <param name="grid">La grille Sudoku.</param>
        /// <param name="row">L'indice de la ligne.</param>
        /// <param name="col">L'indice de la colonne.</param>
        /// <returns>Une liste des valeurs possibles pour cette cellule.</returns>
        private List<int> GetPossibleValues(SudokuGrid grid, int row, int col)
        {
            HashSet<int> usedValues = new HashSet<int>();

            // Ajouter les valeurs utilisées dans la ligne
            for (int c = 0; c < 9; c++)
            {
                if (grid.Cells[row, c] != 0)
                {
                    usedValues.Add(grid.Cells[row, c]);
                }
            }

            // Ajouter les valeurs utilisées dans la colonne
            for (int r = 0; r < 9; r++)
            {
                if (grid.Cells[r, col] != 0)
                {
                    usedValues.Add(grid.Cells[r, col]);
                }
            }

            // Ajouter les valeurs utilisées dans la sous-grille 3x3
            int startRow = (row / 3) * 3;
            int startCol = (col / 3) * 3;

            for (int r = startRow; r < startRow + 3; r++)
            {
                for (int c = startCol; c < startCol + 3; c++)
                {
                    if (grid.Cells[r, c] != 0)
                    {
                        usedValues.Add(grid.Cells[r, c]);
                    }
                }
            }

            // Les valeurs possibles sont celles qui ne sont pas déjà utilisées dans la ligne, la colonne et la sous-grille
            List<int> possibleValues = new List<int>();
            for (int i = 1; i <= 9; i++)
            {
                if (!usedValues.Contains(i))
                {
                    possibleValues.Add(i);
                }
            }

            return possibleValues;
        }
    }
}
