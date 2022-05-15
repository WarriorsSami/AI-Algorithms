from enum import Enum

import networkx as nx
import matplotlib.pyplot as plt


class EdgeColor(Enum):
    OUT_PATH = 'blue'
    IN_PATH = 'red'


def set_edge_color(graph, path, color):
    for edge in path:
        graph.edges[edge]['color'] = color


def draw_graph(graph):
    pos = nx.spring_layout(G, seed=1)
    edges = graph.edges()
    colors = [graph[u][v]['color'] for u, v in edges]

    nx.draw(graph, pos, edge_color=colors, with_labels=True, width=5.0, node_size=1500)
    plt.show()
    plt.pause(0.5)


G = nx.Graph()

with open('data\\cities.txt', 'r') as fin:
    nr_cities = int(fin.readline())
    city_tuples = [fin.readline().split() for _ in range(nr_cities)]
    city_name_dict = {
        tup[1]: {
            'id': int(tup[0]),
            'code': tup[2]
        } for tup in city_tuples
    }
    city_id_dict = {
        int(tup[0]): {
            'name': tup[1],
            'code': tup[2]
        } for tup in city_tuples
    }

    while line := fin.readline():
        line = line.split()
        G.add_edge(city_name_dict[line[0]]['code'],
                   city_name_dict[line[1]]['code'],
                   color=EdgeColor.OUT_PATH.value)
