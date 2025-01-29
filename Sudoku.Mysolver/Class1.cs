using Sudoku.Shared;  
using System;

namespace Sudoku.Mysolver
{
    public class MySolver : ISudokuSolver
    {
        
        public SudokuGrid Solve(SudokuGrid grid)
        {
            
            
            if (SolveSudoku(grid))
            {
                return grid;
            }

            return null; // Retourner null si impossible de résoudre
        }

        // Méthode pour résoudre le Sudoku avec l'algorithme de backtracking (exemple)
        private bool SolveSudoku(SudokuGrid grid)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid[row, col] == 0) // Case vide
                    {
                        for (int num = 1; num <= 9; num++)
                        {
                            if (IsSafe(grid, row, col, num))
                            {
                                grid[row, col] = num;

                                if (SolveSudoku(grid))
                                {
                                    return true;
                                }

                                grid[row, col] = 0; // Revenir en arrière
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        
        private bool IsSafe(SudokuGrid grid, int row, int col, int num)
        {
            // Vérifier la ligne
            for (int x = 0; x < 9; x++)
            {
                if (grid[row, x] == num)
                {
                    return false;
                }
            }

            // Vérifier la colonne
            for (int x = 0; x < 9; x++)
            {
                if (grid[x, col] == num)
                {
                    return false;
                }
            }

            // Vérifier la sous-grille 3x3
            int startRow = row - row % 3;
            int startCol = col - col % 3;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (grid[i + startRow, j + startCol] == num)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
