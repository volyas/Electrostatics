import numpy as np
import matplotlib.pyplot as plt
from matplotlib import patches

path = "..\\InverseProblem\\Results\\"
data = np.loadtxt(path + 'conductivity_11.txt')
rectangles = [(0.0, -10.0, 0.1, 10.0, data[5]),
              (0.1, -4.0, 4.9, 4.0, data[0]),
              (5.0, -5.0, 3.0, 5.0, data[3]),
              (8.0, -4.0, 2.0, 4.0, data[2]),
              (0.1, -5.0, 4.9, 1.0, data[2]),
              (0.1, -6.0, 4.9, 1.0, data[4]),
              (8.0, -5.0, 2.0, 1.0, data[1]),
              (5.0, -6.0, 5.0, 1.0, data[0]),
              (0.1, -10.0, 7.9, 4.0, data[1]),
              (8.0, -10.0, 2.0, 4.0, data[3])]
# rectangles = [(0.0, -260.0, 0.1, 260.0, data[0]),
#               (0.1, -100.0, 100.0, 100.0, data[1]),
#               (0.1, -125.0, 100.0, 25.0, data[2]),
#               (0.1, -130.0, 20.0, 5.0, data[6]),
#               (20.1, -130.0, 80.0, 5.0, data[3]),
#               (0.1, -131.0, 20.0, 1.0, data[4]),
#               (20.1, -131.0, 80.0, 1.0, data[2]),
#               (0.1, -135.0, 100.0, 4.0, data[5]),
#               (0.1, -160.0, 20.0, 25.0, data[2]),
#               (20.1, -160.0, 80.0, 25.0, data[1]),
#               (0.1, -260.0, 100.0, 100.0, data[6])]
# rectangles = [(0.0, -260.0, 0.1, 260.0, data[0]),
#               (0.1, -100.0, 100.0, 100.0, data[1]),
#               (0.1, -130.0, 100.0, 30.0, data[2]),
#               (0.1, -131.0, 20.0, 1.0, data[4]),
#               (20.1, -131.0, 80.0, 1.0, data[3]),
#               (0.1, -160.0, 100.0, 29.0, data[2]),
#               (0.1, -260.0, 100.0, 100.0, data[1])] #(x, y, width, height)
fig, ax = plt.subplots()
for i, rect_data in enumerate(rectangles):
    x, y, width, height, conductivity = rect_data
    rectangle = patches.Rectangle((x, y), width, height, edgecolor='black',
                                  facecolor=plt.cm.rainbow((conductivity - 0) / (0.5 - 0)))
    ax.add_patch(rectangle)

    if i != 0:
        ax.text(x + width / 2, y + height / 2, f'{conductivity:.2f}', ha='center', va='center', color='black')
# ax.set_xlim(0, 50)
# ax.set_ylim(-180, -80)
ax.set_xlim(0, 10) #полные размеры
ax.set_ylim(-10, 0)
plt.xlabel('R, м')
plt.ylabel('Z, м')
ax.set_aspect('equal', adjustable='box')
i = 11
plt.title(f'Исследуемая область, итерация  {i-1}')
sm = plt.cm.ScalarMappable(cmap=plt.cm.rainbow)
sm.set_clim(0, 0.5)
sm.set_array(data)
plt.colorbar(sm, ax=ax, label='Электропроводимость, См')
plt.show()
