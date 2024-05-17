import numpy as np
import matplotlib.pyplot as plt
from matplotlib.patches import Rectangle
import os
import re

# Функция для чтения и обработки данных из файла
def read_potential_differences_from_file(file_path):
    with open(file_path, 'r') as file:
        lines = file.readlines()
        z_coordinate = list(map(float, lines[0].split()))
        measurements = list(map(float, lines[1].split()))
        return z_coordinate, measurements

def read_areas_from_file(file_path):
    with open(file_path, 'r') as file:
        lines = file.readlines()
        areas = []
        for line in lines:
            values = line.split()
            if len(values) == 5:
                r_min, z_min, r_max, z_max, conductivity = map(float, values)
                areas.append({
                    'r': (r_min, r_max),
                    'z': (z_min, z_max),
                    'conductivity': conductivity
                })
        return areas

# Функция для построения графика
def draw_potential_differences_plot_for_iteration(z_coordinate, measurements, iteration_number):
    plt.plot(measurements, z_coordinate, 'o-')
    plt.xlabel('Potential differences')
    plt.ylabel('Z, m')
    plt.title(f'Potential differences on iteration {iteration_number}')
    plt.ylim(-7, -2.5)
    plt.xlim(0, 9)
    plt.show()

def draw_areas_plot_for_iteration(areas, iteration):
    # Создаем фигуру и оси
    fig, ax = plt.subplots()

    # Генерируем данные для подобластей
    for i, subregion in enumerate(areas):
        r = np.linspace(subregion['r'][0], subregion['r'][1], 300)
        z = np.linspace(subregion['z'][0], subregion['z'][1], 300)
        R, Z = np.meshgrid(r, z)
        conductivity = np.full((300, 300), subregion['conductivity'])
        ax.pcolormesh(R, Z, conductivity, shading='auto', cmap='jet_r', vmin=1e-3, vmax=0.5)

        # Отрисовываем прямоугольники для областей
        width = subregion['r'][1] - subregion['r'][0]
        height = subregion['z'][1] - subregion['z'][0]
        rect = Rectangle((subregion['r'][0], subregion['z'][0]), width, height, edgecolor='black', facecolor='none')
        ax.add_patch(rect)

        # Убираем отрисовку значения проводимости для первой подобласти
        if i > 0:
            center_r = (subregion['r'][0] + subregion['r'][1]) / 2
            center_z = (subregion['z'][0] + subregion['z'][1]) / 2
            ax.text(center_r, center_z, f"{subregion['conductivity']:.4}", color='black', ha='center', va='center')

    # Настройки графика
    ax.set_xlabel('R, m')
    ax.set_ylabel('Z, m')
    ax.set_title(f'Areas on iteration {iteration}')
    ax.set_aspect('auto', adjustable='box')
    ax.set_xlim(1e-4, 3)
    ax.set_ylim(-6, 0)

    # Добавляем цветовую шкалу
    sm = plt.cm.ScalarMappable(cmap='jet_r', norm=plt.Normalize(vmin=1e-3, vmax=0.5))
    sm._A = []
    cbar = plt.colorbar(sm, ax=ax, label='Conductivity, S')

    # Отображаем график
    plt.show()

def draw_plot_for_true_values(z_coordinate, measurements):
    plt.plot(measurements, z_coordinate, 'o-')
    plt.xlabel('Potential differences')
    plt.ylabel('Z, m')
    plt.title(f'Potential differences true')
    plt.ylim(-7, -2.5)
    plt.show()

def draw_areas_plot_for_true_values(areas):
    # Создаем фигуру и оси
    fig, ax = plt.subplots()

    # Генерируем данные для подобластей
    for i, subregion in enumerate(areas):
        r = np.linspace(subregion['r'][0], subregion['r'][1], 300)
        z = np.linspace(subregion['z'][0], subregion['z'][1], 300)
        R, Z = np.meshgrid(r, z)
        conductivity = np.full((300, 300), subregion['conductivity'])
        ax.pcolormesh(R, Z, conductivity, shading='auto', cmap='jet_r', vmin=1e-3, vmax=0.5)

        # Отрисовываем прямоугольники для областей
        width = subregion['r'][1] - subregion['r'][0]
        height = subregion['z'][1] - subregion['z'][0]
        rect = Rectangle((subregion['r'][0], subregion['z'][0]), width, height, edgecolor='black', facecolor='none')
        ax.add_patch(rect)

        # Убираем отрисовку значения проводимости для первой подобласти
        if i > 0:
            center_r = (subregion['r'][0] + subregion['r'][1]) / 2
            center_z = (subregion['z'][0] + subregion['z'][1]) / 2
            ax.text(center_r, center_z, f"{subregion['conductivity']:.4}", color='black', ha='center', va='center')

    # Настройки графика
    ax.set_xlabel('R, m')
    ax.set_ylabel('Z, m')
    ax.set_title(f'Areas true')
    ax.set_aspect('auto', adjustable='box')
    ax.set_xlim(1e-4, 3)
    ax.set_ylim(-6, 0)

    # Добавляем цветовую шкалу
    sm = plt.cm.ScalarMappable(cmap='jet_r', norm=plt.Normalize(vmin=1e-3, vmax=0.5))
    sm._A = []
    cbar = plt.colorbar(sm, ax=ax, label='Conductivity, S')

    # Отображаем график
    plt.show()

# Директория, откуда нужно считать файлы
directory = "..\\InverseProblem\\Results\\"
# directory = "..\\InverseProblem\\Results\\3 v\\"
# Обработка каждого файла в директории
for file_name in os.listdir(directory):
    match = re.search(r'_(\d+)', file_name)
    if file_name.startswith('potentialDifferencesIteration_'):
        # Извлечение номера итерации из названия файла
        file_path = os.path.join(directory, file_name)
        z_coordinate, measurements = read_potential_differences_from_file(file_path)
        if match:
            iteration_number = int(match.group(1))
            draw_potential_differences_plot_for_iteration(z_coordinate, measurements, iteration_number)
        elif file_name == 'potentialDifferencesIteration_true.txt':
            draw_plot_for_true_values(z_coordinate, measurements)
    elif file_name.startswith('areas.txt'):
        file_path = os.path.join(directory, file_name)
        areas = read_areas_from_file(file_path)
        if match:
            iteration_number = int(match.group(1))
            draw_areas_plot_for_iteration(areas, iteration_number)
        elif file_name == 'true areas.txt':
            draw_areas_plot_for_true_values(areas)