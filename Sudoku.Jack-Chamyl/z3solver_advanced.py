from z3 import Solver, Int, And, Distinct, sat

def solve_sudoku(instance):
    """
    Résout un Sudoku 9x9 en utilisant la substitution pour appliquer les valeurs fixes.
    
    Paramètre:
      instance: une séquence (tuple ou liste) de 9 séquences de 9 entiers représentant la grille,
                où 0 indique une case vide.
    
    Retourne:
      Un tuple (status, solution) où:
       - status est "sat" si une solution a été trouvée, sinon "unsat",
       - solution est la grille résolue (liste de listes 9x9) si status=="sat", sinon None.
    """
    solver = Solver()

    # Création d'une matrice 9x9 de variables entières.
    cells = [[Int(f"cell_{i}_{j}") for j in range(9)] for i in range(9)]

    # Contraintes pour chaque cellule : valeur entre 1 et 9.
    for i in range(9):
        for j in range(9):
            solver.add(And(cells[i][j] >= 1, cells[i][j] <= 9))

    # Contraintes sur les lignes : chaque ligne contient des valeurs distinctes.
    for i in range(9):
        solver.add(Distinct(cells[i]))

    # Contraintes sur les colonnes : chaque colonne contient des valeurs distinctes.
    for j in range(9):
        solver.add(Distinct([cells[i][j] for i in range(9)]))

    # Contraintes sur les blocs 3x3 : chaque bloc contient des valeurs distinctes.
    for box_row in range(3):
        for box_col in range(3):
            block = [cells[box_row * 3 + i][box_col * 3 + j] for i in range(3) for j in range(3)]
            solver.add(Distinct(block))

    # Construction d'une liste de contraintes de substitution pour les valeurs fixes.
    substitutions = [
        cells[i][j] == instance[i][j]
        for i in range(9)
        for j in range(9)
        if instance[i][j] != 0
    ]
    # Application de toutes les substitutions en une seule instruction.
    solver.add(substitutions)

    if solver.check() == sat:
        model = solver.model()
        solution = [[model.evaluate(cells[i][j]).as_long() for j in range(9)] for i in range(9)]
        return ("sat", solution)
    else:
        return ("unsat", None)

# Test local (pour exécution directe)
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
    status, sol = solve_sudoku(instance)
    print(status)
    if status == "sat":
        for row in sol:
            print(row)