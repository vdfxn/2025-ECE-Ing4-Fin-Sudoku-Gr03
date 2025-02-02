import sys
from itertools import product, combinations
from timeit import default_timer
from z3 import Solver, FreshBool, And, Or, Not, sat, set_param
set_param('model', False)

##############################################################################
# Petit utilitaire: exactly_one (retourne des clauses "au moins un" + "au plus un")

def exactly_one_clauses(lits):
    """
    Produit la liste de formules imposant que
    exactement un littéral de 'lits' soit vrai.
      - Au moins un: Or(lits)
      - Au plus un:  pour toute paire (x,y), Not(x && y)
    """
    # Au moins un
    c = [Or(*lits)]
    # Au plus un (paires)
    c.extend(Not(And(x, y)) for x, y in combinations(lits, 2))
    return c

def solve_sudoku_fast_no_model(grid):
    """
    Résout un Sudoku 9×9 via un encodage SAT en Z3, en Python,
    en essayant de minimiser tout overhead.
    
    - Utilise FreshBool() pour éviter de fabriquer des noms de variables.
    - Construit toutes les clauses puis effectue UNE SEULE fois solver.add(*...).
    - Ne renvoie PAS la solution (pas d'extraction de modèle).
    
    Retourne le temps (en ms) passé uniquement dans solver.check().
    """
    # On fixe un seed pour limiter l'aléatoire (facultatif)
    # set_param('random_seed', 0)

    solver = Solver()
    # Parfois utile :
    solver.set(threads=4, timeout=10_000)  # Timeout 10s, 4 cœurs

    # Création des variables : x[i][j][d] = FreshBool()
    # True <=> la case (i,j) est la valeur d+1
    x = [[[FreshBool() for d in range(9)]
          for j in range(9)]
         for i in range(9)]

    constraints = []

    # 1) EXACTEMENT UNE VALEUR PAR CASE
    for i, j in product(range(9), range(9)):
        lits_case = x[i][j]  # 9 FreshBool
        constraints.extend(exactly_one_clauses(lits_case))

    # 2) CHAQUE VALEUR D APPARAÎT EXACTEMENT UNE FOIS PAR LIGNE
    for i in range(9):
        for d in range(9):
            lits_line = [x[i][j][d] for j in range(9)]
            constraints.extend(exactly_one_clauses(lits_line))

    # 3) CHAQUE VALEUR D APPARAÎT EXACTEMENT UNE FOIS PAR COLONNE
    for j in range(9):
        for d in range(9):
            lits_col = [x[i][j][d] for i in range(9)]
            constraints.extend(exactly_one_clauses(lits_col))

    # 4) CHAQUE VALEUR D APPARAÎT EXACTEMENT UNE FOIS PAR BLOC 3x3
    for box_i in range(3):
        for box_j in range(3):
            for d in range(9):
                block = []
                for di in range(3):
                    for dj in range(3):
                        i_ = 3*box_i + di
                        j_ = 3*box_j + dj
                        block.append(x[i_][j_][d])
                constraints.extend(exactly_one_clauses(block))

    # 5) CONTRAINTES D'INITIALISATION
    for i, j in product(range(9), range(9)):
        val = grid[i][j]
        if val != 0:
            # d = val-1 doit être True, les autres False
            d_ok = val - 1
            for d in range(9):
                if d == d_ok:
                    constraints.append(x[i][j][d])     # doit être True
                else:
                    constraints.append(Not(x[i][j][d]))  # doit être False

    # Ajout en bloc
    solver.add(*constraints)

    # === Mesure du temps de résolution (uniquement solver.check()) ===
    t0 = default_timer()
    check_res = solver.check()
    t1 = default_timer()
    solve_time_ms = (t1 - t0)*1000.0

    if check_res == sat:
        # On ne lit PAS le modèle pour éviter overhead !
        return solve_time_ms
    else:
        # Sudoku standard => pas censé être UNSAT s'il est correct
        return None

##############################################################################
# Exemple
if __name__ == "__main__":

    # Sudoku exemple
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

    # On mesure tout (construction + check)
    t0_all = default_timer()
    check_time = solve_sudoku_fast_no_model(instance)
    t1_all = default_timer()
    total_ms = (t1_all - t0_all)*1000.0

    if check_time is not None:
        print(f"Z3 check() = {check_time:.2f} ms")
        print(f"Temps total Python (construction + check) = {total_ms:.2f} ms")
    else:
        print("Aucune solution trouvée (ce qui est surprenant pour un Sudoku correct).")