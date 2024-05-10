import numpy as np
import matplotlib.pyplot as plt
from matplotlib import patches

path = "..\\InverseProblem\\Results\\"
data = np.loadtxt(path + 'conductivity_1.txt')
rectangles = [(0.0, -260.0, 0.1, 260.0, data[0]),
              (0.1, -100.0, 100.0, 100.0, data[1]),
              (0.1, -130.0, 100.0, 30.0, data[2]),
              (0.1, -131.0, 20.0, 1.0, data[4]),
              (20.1, -131.0, 80.0, 1.0, data[3]),
              (0.1, -160.0, 100.0, 29.0, data[2]),
              (0.1, -260.0, 100.0, 100.0, data[1])] #(x, y, width, height)
fig, ax = plt.subplots()
for rect_data in rectangles:
    x, y, width, height, conductivity = rect_data
    rectangle = patches.Rectangle((x, y), width, height, edgecolor='black', facecolor=plt.cm.rainbow((conductivity-0)/(0.5-0)))
    ax.add_patch(rectangle)
    ax.text(x + width/2, y + height/2, f'{conductivity:.2f}', ha='center', va='center', color='black')
# ax.set_xlim(0, 50)
# ax.set_ylim(-180, -80)
ax.set_xlim(0, 100) #полные размеры
ax.set_ylim(-260, 0)
plt.xlabel('R, м')
plt.ylabel('Z, м')
ax.set_aspect('equal', adjustable='box')
i = 1
plt.title(f'Исследуемая область, итерация  {i}')
sm = plt.cm.ScalarMappable(cmap=plt.cm.rainbow)
sm.set_clim(0, 0.5)
sm.set_array(data)
plt.colorbar(sm, ax=ax, label='Электропроводимость, См')
plt.show()