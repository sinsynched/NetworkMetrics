import csv
from turtle import width
import matplotlib.pyplot as plt
import matplotlib.gridspec as gridspec


# Initialize lists to store the data
x = []
y = []

# Read the CSV file which has degree distibution data
with open('DegreeDistribution.csv', 'r') as csvfile:
    csvreader = csv.reader(csvfile)
    for row in csvreader:
        x.append(float(row[0]))
        y.append(float(row[1]))


# Create the figure and GridSpec layout
fig = plt.figure(figsize=(12, 6))
# 3 units for plot, 1 unit for info
gs = gridspec.GridSpec(1, 2, width_ratios=[1, 1.7], wspace=0.05, hspace=0)

# Create the plot subplot
subPlot1 = plt.subplot(gs[0])

# Plot the data
subPlot1.plot(x, y, marker='o')

# Set plot title and labels
subPlot1.set_title('Degree Distribution')
subPlot1.set_xlabel('K')
subPlot1.set_ylabel('Frequency')
subPlot1.set_box_aspect(1)

# Create the information text subplot
subPlot2 = plt.subplot(gs[1])
subPlot2.axis('off')  # Hide axes

data = []

with open('NetworkMetrics.csv', 'r') as csvfile:
    csvreader = csv.reader(csvfile)
    for row in csvreader:
        data.append(row)

# Create a table and add it to the plot
table = subPlot2.table(cellText=data, loc='center', cellLoc='center')

# Adjust vertical scaling factor
table.scale(1, 2)

table.auto_set_font_size(False)
table.set_fontsize(9.8)

# Remove margins
plt.subplots_adjust(left=0.07, right=0.99)

# Show the plot
plt.savefig('DegreeDistributionAndMetrics.png', dpi=300)
plt.show()
