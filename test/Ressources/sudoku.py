import copy  # Nécessaire pour effectuer des copies indépendantes des objets.
from timeit import default_timer
import numpy as np  # Bibliothèque pour les calculs numériques.
from tensorflow import keras
# Chargement des données d'entraînement et de test à partir d'un fichier CSV.
#x_train, x_test, y_train, y_test = get_data('sudoku1.csv')

# Initialisation du modèle.
#model = get_model()

# Configuration de l'optimiseur Adam avec un taux d'apprentissage de 0.001.
#adam = keras.optimizers.Adam(learning_rate=0.001)
# Compilation du modèle avec une fonction de perte pour la classification multi-classes.
#model.compile(loss='sparse_categorical_crossentropy', optimizer=adam)

# Entraînement du modèle sur les données d'entraînement.
#model.fit(x_train, y_train, batch_size=32, epochs=2)

model = keras.models.load_model('my_model1.keras')

# Fonction pour normaliser les données d'entrée (valeurs entre -0.5 et 0.5).
def norm(a):
    return (a / 9) - .5



# Fonction pour dénormaliser les données en revenant à la plage d'origine.
def denorm(a):
    return (a + .5) * 9

# Fonction pour résoudre un Sudoku en remplissant les cases vides une par une.


def inference_sudoku(sample):
    '''
    This function solves the sudoku by filling blank positions one by one.
    '''

    feat = copy.copy(sample)  # Copie indépendante de l'échantillon pour préserver les données d'origine.

    while (1):  # Boucle jusqu'à ce que toutes les cases soient remplies.
        out = model.predict(feat.reshape((1, 9, 9, 1)))  # Prédiction des probabilités pour chaque case.
        out = out.squeeze()  # Réduction des dimensions de la sortie.

        pred = np.argmax(out, axis=1).reshape((9, 9)) + 1  # Classe prédite pour chaque case.
        prob = np.around(np.max(out, axis=1).reshape((9, 9)), 2)  # Probabilité associée à chaque prédiction.

        feat = denorm(feat).reshape((9, 9))  # Dénormalisation pour manipulation plus lisible.
        mask = (feat == 0)  # Masque indiquant les cases vides.

        if (mask.sum() == 0):  # Si aucune case vide, le Sudoku est résolu.
            break

        prob_new = prob * mask  # Probabilités appliquées uniquement aux cases vides.

        ind = np.argmax(prob_new)  # Index de la case avec la probabilité maximale.
        x, y = (ind // 9), (ind % 9)  # Conversion de l'index en coordonnées (x, y).

        val = pred[x][y]  # Valeur prédite pour la case.
        feat[x][y] = val  # Remplissage de la case avec la valeur prédite.
        feat = norm(feat)  # Normalisation pour les prochaines prédictions.

    return pred  # Retourne la grille résolue.



# Fonction pour tester l'exactitude du modèle sur un échantillon de données.
def test_accuracy(feats, labels):
    correct = 0  # Compteur des prédictions correctes.

    for i, feat in enumerate(feats):  # Parcourt chaque échantillon de test.
        pred = inference_sudoku(feat)  # Résolution du Sudoku.

        true = labels[i].reshape((9, 9)) + 1  # Grille solution réelle.

        if (abs(true - pred).sum() == 0):  # Vérification de l'exactitude.
            correct += 1

    print(correct / feats.shape[0])  # Affichage du pourcentage de réussite.

# Test de l'exactitude sur 100 grilles de Sudoku.
#test_accuracy(x_test[:100], y_test[:100])



# Fonction pour résoudre une grille de Sudoku donnée sous forme de texte.
def solve_sudoku(game):
    game = game.replace('\n', '')  # Suppression des sauts de ligne.
    game = game.replace(' ', '')  # Suppression des espaces.
    game = np.array([int(j) for j in game]).reshape((9, 9, 1))  # Conversion en tableau NumPy.
    game = norm(game)  # Normalisation de la grille.
    game = inference_sudoku(game)  # Résolution de la grille.
    return game  # Retourne la grille résolue.


#test
# Exemple de résolution d'une grille de Sudoku donnée sous forme brute.
game = '''
          0 8 0 0 3 2 0 0 1
          7 0 3 0 8 0 0 0 2
          5 0 0 0 0 7 0 3 0
          0 5 0 0 0 1 9 7 0
          6 0 0 7 0 9 0 0 8
          0 4 7 2 0 0 0 5 0
          0 2 0 6 0 0 0 0 9
          8 0 0 0 9 0 3 0 5
          3 0 0 8 2 0 0 1 0
      '''

start = default_timer()
game = solve_sudoku(game)  # Résolution de la grille.

print('solved puzzle:\n')  # Affichage du puzzle résolu.
print(game)
np.sum(game, axis=1)  # Somme des lignes pour vérifier l'intégrité du Sudoku.

execution = default_timer() - start
print("Le temps de résolution est de : ", execution * 1000, " ms")

#model.save('my_model1.h5')  # Sauvegarde au format HDF5
