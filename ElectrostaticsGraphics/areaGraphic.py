import numpy as np
import matplotlib.pyplot as plt

rectangles = [(0.0, 0.0, 2.0, 3.0), (3.0, 2.0, 4.0, 1.0), (1.0, 5.0, 2.0, 2.0)]
plt.figure(figsize=(8, 6)) # Пример: каждый прямоугольник задается как (x, y, width, height)
for i in range(3):
    data = np.loadtxt(f'data_{i}.txt')

    for rect in rectangles:
        x, y, width, height = rect
        conductivity_value = data[rect[1]:rect[1] + rect[3],
                             rect[0]:rect[0] + rect[2]]  # Значения проводимостей внутри прямоугольника
        plt.imshow(conductivity_value,
                   cmap='viridis',
                   interpolation='nearest',
                   extent=[x, x + width, y, y + height],
                   vmin=0,
                   vmax=0.5)

        for row in range(height):
            for col in range(width):
                plt.text(x + col + 0.5, y + row + 0.5, str(int(conductivity_value[row, col])), color='black',
                         ha='center', va='center')

        plt.colorbar(orientatin='horizontal', label='Электропроводимости')

        plt.xlim(-135, -127)
        plt.ylim(0.5, 1.5)

        plt.xlabel('R, м')
        plt.ylabel('Z, м')
        plt.title(f'Исследуемая область, итерация  {i}')
        plt.show()