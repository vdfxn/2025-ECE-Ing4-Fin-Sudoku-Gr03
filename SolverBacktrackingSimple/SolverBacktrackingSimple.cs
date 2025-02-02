using Sudoku.Shared;

namespace SolverBacktrackingSimple;

public class SolverBacktrackingSimple : ISudokuSolver
{
private const int SIZE = 9;
        private bool[,] rowUsed = new bool[SIZE, SIZE + 1];
        private bool[,] colUsed = new bool[SIZE, SIZE + 1];
        private bool[,] boxUsed = new bool[SIZE, SIZE + 1];
        private int callCount = 0;

        public SudokuGrid Solve(SudokuGrid s)
        {
            InitializeConstraints(s);
            callCount = 0;
            if (Search(s, 0, 0))
            {
                Console.WriteLine("BacktrackingDotNetSolver: " + callCount + " search calls");
                return s;
            }
            else
            {
                throw new InvalidOperationException("No solution exists for the given Sudoku grid");
            }
        }

        private void InitializeConstraints(SudokuGrid s)
        {
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    int num = s.Cells[i, j];
                    if (num != 0)
                    {
                        rowUsed[i, num] = true;
                        colUsed[j, num] = true;
                        boxUsed[GetBoxIndex(i, j), num] = true;
                    }
                }
            }
        }

        private bool Search(SudokuGrid s, int row, int col)
        {
            callCount++;
            if (col == SIZE)
            {
                col = 0; ++row;
                if (row == SIZE) return true;
            }
            if (s.Cells[row, col] != 0)
                return Search(s, row, col + 1);

            for (int num = 1; num <= SIZE; num++)
            {
                if (IsValid(row, col, num))
                {
                    s.Cells[row, col] = num;
                    SetConstraints(row, col, num, true);

                    if (Search(s, row, col + 1)) return true;

                    s.Cells[row, col] = 0;
                    SetConstraints(row, col, num, false);
                }
            }
            return false;
        }

        private bool IsValid(int row, int col, int num)
        {
            return !rowUsed[row, num] && !colUsed[col, num] && !boxUsed[GetBoxIndex(row, col), num];
        }

        private void SetConstraints(int row, int col, int num, bool state)
        {
            rowUsed[row, num] = state;
            colUsed[col, num] = state;
            boxUsed[GetBoxIndex(row, col), num] = state;
        }

        private int GetBoxIndex(int row, int col)
        {
            return (row / 3) * 3 + (col / 3);
        }
    }
