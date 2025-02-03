import clr
clr.AddReference("Sudoku.Shared")
clr.AddReference("Sudoku.Pierrelouis")
from Sudoku.Pierrelouis import PierrelouisDotNetSolver
netSolver = PierrelouisDotNetSolver()
solvedSudoku = netSolver.Solve(sudoku)