using Sudoku.Shared;
using Microsoft.Z3;
namespace Sudoku.Z3Solvers;

public class Z3SimpleSolver : ISudokuSolver
{
	public SudokuGrid Solve(SudokuGrid s)
	{
		return s;
	}
}