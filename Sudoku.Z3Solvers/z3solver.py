from z3 import Solver, Bool, PbEq, sat, set_param
from itertools import product
from timeit import default_timer

def solve_sudoku_sat(grid):
    """
    Encodage SAT (booléen) du Sudoku avec Z3, en utilisant PbEq
    et divers réglages pour accélérer la résolution.
    
    grid : 9x9 avec 0 pour case vide, [1..9] sinon
    Retour : grille résolue ou None
    """
    # Paramètres globaux pour activer le parallélisme et quelques optimisations
    # dans les formules SAT/boolean. 
    set_param("parallel.enable", True)

    solver = Solver()
    # Autoriser plusieurs threads si possible (si votre version de Z3 le supporte).
    solver.set("threads", 4)

    # NOTE : Certaines versions de Z3 ne prennent pas en charge le paramètre "sat.pb.solver".
    # Si c'est le cas, retirez simplement la ligne correspondante.
    # solver.set("sat.pb.solver", True)  # <-- Ligne retirée pour éviter l'erreur "unknown parameter"

    # Limite de temps (en millisecondes, ici ~10s)
    solver.set("timeout", 10_000)

    # Variables booléennes : x[i][j][d] => la case (i,j) contient d+1
    x = [[[Bool(f"x_{i}_{j}_{d}")
            for d in range(9)]
            for j in range(9)]
            for i in range(9)]

    #
    # 1) Contrainte : chaque case (i, j) a exactement 1 des 9 valeurs
    #
    for i, j in product(range(9), range(9)):
        solver.add(PbEq([(x[i][j][d], 1) for d in range(9)], 1))

    #
    # 2) Chaque ligne i, pour chaque valeur d, doit l'avoir exactement 1 fois
    #
    for i in range(9):
        for d in range(9):
            solver.add(PbEq([(x[i][j][d], 1) for j in range(9)], 1))

    #
    # 3) Chaque colonne j, pour chaque valeur d, doit l'avoir exactement 1 fois
    #
    for j in range(9):
        for d in range(9):
            solver.add(PbEq([(x[i][j][d], 1) for i in range(9)], 1))

    #
    # 4) Chaque boîte 3x3, pour chaque valeur d, doit l'avoir exactement 1 fois
    #
    for box_i in range(3):
        for box_j in range(3):
            for d in range(9):
                box_vars = []
                for di in range(3):
                    for dj in range(3):
                        i = 3*box_i + di
                        j = 3*box_j + dj
                        box_vars.append((x[i][j][d], 1))
                solver.add(PbEq(box_vars, 1))

    #
    # 5) Contraintes initiales (grille partiellement remplie)
    #
    for i, j in product(range(9), range(9)):
        val = grid[i][j]
        if val != 0:
            d = val - 1  # 0..8
            # Forcer x[i][j][val-1] = True et le reste = False
            for dd in range(9):
                solver.add(x[i][j][dd] == (dd == d))

    start_time = default_timer()
    is_sat = solver.check()
    end_time = default_timer()

    if is_sat == sat:
        model = solver.model()
        duration = (end_time - start_time) * 1000
        print(f"Solution trouvée en {duration:.2f} ms")

        # Extraire la solution
        solution = [[0]*9 for _ in range(9)]
        for i, j, d in product(range(9), range(9), range(9)):
            if model[x[i][j][d]]:
                solution[i][j] = d + 1
        return solution
    else:
        print("Aucune solution trouvée.")
        return None

# __main__ pour démonstration
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

    start_global = default_timer()
    solution = solve_sudoku_sat(instance)
    end_global = default_timer()

    if solution:
        print("Sudoku résolu :")
        for row in solution:
            print(row)
        print(f"Temps total = {(end_global - start_global)*1000:.2f} ms")
    else:
        print("Pas de solution.")