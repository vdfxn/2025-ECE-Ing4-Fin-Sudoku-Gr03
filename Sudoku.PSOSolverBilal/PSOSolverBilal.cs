using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.PSOSolverBilal
{
    public class PSOSolverBilal : ISudokuSolver
    {
        private const int Size = 9;
        private const int PopulationSize = 50;
        private const int MaxIterations = 1000;
        private const double Inertia = 0.7;
        private const double Cognitive = 1.5;
        private const double Social = 1.5;

        private class Particle
        {
            public int[,] Position;
            public int[,] BestPosition;
            public double Fitness;
            public double BestFitness;
        }

        public SudokuGrid Solve(SudokuGrid grid)
        {
            List<Particle> swarm = InitializeSwarm(grid);
            Particle globalBest = swarm.OrderBy(p => p.Fitness).First();

            for (int iter = 0; iter < MaxIterations; iter++)
            {
                foreach (var particle in swarm)
                {
                    UpdateParticle(particle, globalBest);
                    if (particle.Fitness < globalBest.Fitness)
                    {
                        globalBest = particle;
                    }
                }
            }

            return ConvertToSudokuGrid(globalBest.Position);
        }

        private List<Particle> InitializeSwarm(SudokuGrid grid)
        {
            List<Particle> swarm = new List<Particle>();
            for (int i = 0; i < PopulationSize; i++)
            {
                Particle particle = new Particle();
                particle.Position = GenerateValidSolution(grid);
                particle.BestPosition = (int[,])particle.Position.Clone();
                particle.Fitness = ComputeFitness(particle.Position);
                particle.BestFitness = particle.Fitness;
                swarm.Add(particle);
            }
            return swarm;
        }

        private void UpdateParticle(Particle particle, Particle globalBest)
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (particle.Position[row, col] == 0)
                    {
                        int newValue = RandomlyMutateValue(row, col, particle, globalBest);
                        particle.Position[row, col] = newValue;
                    }
                }
            }
            particle.Fitness = ComputeFitness(particle.Position);
            if (particle.Fitness < particle.BestFitness)
            {
                particle.BestPosition = (int[,])particle.Position.Clone();
                particle.BestFitness = particle.Fitness;
            }
        }

        private int[,] GenerateValidSolution(SudokuGrid grid)
        {
            int[,] solution = new int[Size, Size];
            Random rand = new Random();
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    solution[row, col] = grid.Cells[row][col] != 0 ? grid.Cells[row][col] : rand.Next(1, 10);
                }
            }
            return solution;
        }

        private int RandomlyMutateValue(int row, int col, Particle particle, Particle globalBest)
        {
            Random rand = new Random();
            return rand.Next(1, 10);
        }

        private double ComputeFitness(int[,] board)
        {
            int conflicts = 0;
            for (int i = 0; i < Size; i++)
            {
                conflicts += CountConflicts(board, i, true);
                conflicts += CountConflicts(board, i, false);
            }
            return conflicts;
        }

        private int CountConflicts(int[,] board, int index, bool isRow)
        {
            HashSet<int> seen = new HashSet<int>();
            int conflicts = 0;
            for (int i = 0; i < Size; i++)
            {
                int value = isRow ? board[index, i] : board[i, index];
                if (seen.Contains(value))
                {
                    conflicts++;
                }
                else
                {
                    seen.Add(value);
                }
            }
            return conflicts;
        }

        private SudokuGrid ConvertToSudokuGrid(int[,] board)
        {
            SudokuGrid grid = new SudokuGrid();
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    grid.Cells[row][col] = board[row, col];
                }
            }
            return grid;
        }
    }
}
