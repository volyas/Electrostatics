import matplotlib.pyplot as plt

path = "..\\DirectProblem\\Results\\"

with open(path + 'potentialDifferences.txt', 'r') as file:
    points = [float(x) for x in file.readline().split()]
    potentialDifferences = [float(x) for x in file.readline().split()]

plt.plot(points, potentialDifferences, label='potentialDifferences')

plt.xlabel('Z')
plt.ylabel('Diff')

plt.xlim(-135, -127)
plt.ylim(0.5, 1.5)
##plt.xlim(-100, -159)

plt.legend()

plt.show()