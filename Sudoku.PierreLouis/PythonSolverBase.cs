using Sudoku.Shared;
using System.Diagnostics;

namespace Sudoku.PierreLouis
{
    public abstract class PythonSolverBase : ISudokuSolver
    {
        private readonly string _pythonScript;

        protected PythonSolverBase(string pythonScript)
        {
            _pythonScript = pythonScript;
        }

        public SudokuGrid Solve(SudokuGrid s)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "python3", // ou "python" selon ton système
                Arguments = $"{_pythonScript} {s.ToString()}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
                var output = process.StandardOutput.ReadToEnd();
                return ParseOutput(output);
            }
        }

        private SudokuGrid ParseOutput(string output)
        {
            // Implémente la logique pour convertir la sortie Python en une grille Sudoku
            return SudokuGrid.ReadSudoku(output);
        }
    }
}