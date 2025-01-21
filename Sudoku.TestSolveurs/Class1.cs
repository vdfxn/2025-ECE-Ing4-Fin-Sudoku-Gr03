using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.TestSolveurs;

public class TestSolveursTests : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        return s;
    }
}
