from itertools import product, combinations
from z3 import Solver, Bool, And, Or, Not, sat
from timeit import default_timer

class Z3SolverBase:
    def __init__(self, grid, timeout=10_000, threads=4):
        self.grid = grid
        self.solver = Solver()
        self.solver.set(timeout=timeout)
        self.solver.set(threads=threads)
        # Création des variables booléennes
        self.x = [[[Bool(f"x_{i}_{j}_{d}") for d in range(9)]
                   for j in range(9)] for i in range(9)]
        self.constraints = []
        self.add_cell_constraints()
        self.add_row_constraints()
        self.add_col_constraints()
        self.add_block_constraints()
        self.add_given_constraints()
        self.solver.add(*self.constraints)

    def add_cell_constraints(self):
        # Chaque case doit contenir exactement une valeur.
        for i, j in product(range(9), range(9)):
            # Au moins une valeur
            self.constraints.append(Or(*(self.x[i][j][d] for d in range(9))))
            # Au plus une valeur (exclusion pairwise)
            for d1, d2 in combinations(range(9), 2):
                self.constraints.append(Not(And(self.x[i][j][d1], self.x[i][j][d2])))

    def add_row_constraints(self):
        # Chaque chiffre apparaît exactement une fois par ligne.
        for i in range(9):
            for d in range(9):
                self.constraints.append(Or(*(self.x[i][j][d] for j in range(9))))
                for j1, j2 in combinations(range(9), 2):
                    self.constraints.append(Not(And(self.x[i][j1][d], self.x[i][j2][d])))

    def add_col_constraints(self):
        # Chaque chiffre apparaît exactement une fois par colonne.
        for j in range(9):
            for d in range(9):
                self.constraints.append(Or(*(self.x[i][j][d] for i in range(9))))
                for i1, i2 in combinations(range(9), 2):
                    self.constraints.append(Not(And(self.x[i1][j][d], self.x[i2][j][d])))

    def add_block_constraints(self):
        # Chaque chiffre apparaît exactement une fois par bloc 3x3.
        for box_i in range(3):
            for box_j in range(3):
                for d in range(9):
                    block_vars = []
                    for di in range(3):
                        for dj in range(3):
                            i_ = 3 * box_i + di
                            j_ = 3 * box_j + dj
                            block_vars.append(self.x[i_][j_][d])
                    self.constraints.append(Or(*block_vars))
                    for v1, v2 in combinations(block_vars, 2):
                        self.constraints.append(Not(And(v1, v2)))

    def add_given_constraints(self):
        # Contraintes pour les cases déjà remplies dans la grille.
        for i, j in product(range(9), range(9)):
            val = self.grid[i][j]
            if val != 0:
                for d in range(9):
                    if d == val - 1:
                        self.constraints.append(self.x[i][j][d])
                    else:
                        self.constraints.append(Not(self.x[i][j][d]))

    def solve(self):
        start_time = default_timer()
        result = self.solver.check()
        end_time = default_timer()
        duration_ms = (end_time - start_time) * 1000
        if result == sat:
            print(f"Solution trouvée en {duration_ms:.2f} ms")
            model = self.solver.model()
            solution = [[0]*9 for _ in range(9)]
            for i, j, d in product(range(9), range(9), range(9)):
                if model[self.x[i][j][d]]:
                    solution[i][j] = d + 1
            return solution
        else:
            print("Aucune solution trouvée.")
            return None

# Exemple d'utilisation :
if __name__ == "__main__":
    instance = (
        (0, 0, 0, 0, 9, 4, 0, 3, 0),
        (0, 0, 0, 5, 1, 0, 0, 0, 7),
        (0, 8, 9, 0, 0, 0, 0, 4, 0),
        (0, 0, 0, 0, 0, 0, 2, 0, 8),
        (0, 6, 0, 2, 0, 1, 0, 5, 0),
        (1, 0, 2, 0, 0, 0, 0, 0, 0),
        (0, 7, 0, 0, 0, 0, 5, 2, 0),
        (9, 0, 0, 0, 6, 5, 0, 0, 0),
        (0, 4, 0, 9, 7, 0, 0, 0, 0),
    )
    solver = Z3SolverBase(instance)
    solution = solver.solve()
    if solution:
        print("Sudoku résolu :")
        for row in solution:
            print(row)