import numpy as np
from timeit import default_timer

import copy
import keras
import numpy as np
from timeit import default_timer

# Import le modèle train
model = keras.models.load_model('sudoku.model.keras')

# Définir `instance` uniquement si non déjà défini par PythonNET
if 'instance' not in locals():
    instance = np.array([
        [0,0,0,0,9,4,0,3,0],
        [0,0,0,5,1,0,0,0,7],
        [0,8,9,0,0,0,0,4,0],
        [0,0,0,0,0,0,2,0,8],
        [0,6,0,2,0,1,0,5,0],
        [1,0,2,0,0,0,0,0,0],
        [0,7,0,0,0,0,5,2,0],
        [9,0,0,0,6,5,0,0,0],
        [0,4,0,9,7,0,0,0,0]
    ], dtype=int)
def norm(a):
    return (a / 9) - .5

def denorm(a):
    return (a + .5) * 9
def inference_sudoku(sample):
    '''
        This function solve the sudoku by filling blank positions one by one.
    '''

    feat = copy.copy(sample)

    while (1):

        out = model.predict(feat.reshape((1, 9, 9, 1)))
        out = out.squeeze()

        pred = np.argmax(out, axis=1).reshape((9, 9)) + 1
        prob = np.around(np.max(out, axis=1).reshape((9, 9)), 2)

        feat = denorm(feat).reshape((9, 9))
        mask = (feat == 0)

        if (mask.sum() == 0):
            break

        prob_new = prob * mask

        ind = np.argmax(prob_new)
        x, y = (ind // 9), (ind % 9)

        val = pred[x][y]
        feat[x][y] = val
        feat = norm(feat)

    return pred

def solve_sudoku(instance):
    instance = instance.replace('\n', '')
    instance = instance.replace(' ', '')
    instance = np.array([int(j) for j in instance]).reshape((9, 9, 1))
    instance = norm(instance)
    instance = inference_sudoku(instance)
    return instance
start = default_timer()
instance = solve_sudoku(instance)  # Résolution de la grille.

print('solved puzzle:\n')  # Affichage du puzzle résolu.
print(instance)
np.sum(instance, axis=1)  # Somme des lignes pour vérifier l'intégrité du Sudoku.

start = default_timer()
# Exécuter la résolution de Sudoku
if solve_sudoku(instance):
    # print("Sudoku résolu par backtracking avec succès.")
    result = instance  # `result` sera utilisé pour récupérer la grille résolue depuis C#
else:
    print("Aucune solution trouvée.")
execution = default_timer() - start
print("Le temps de résolution est de : ", execution * 1000, " ms")