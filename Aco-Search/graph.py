from enum import Enum

import networkx as nx
import matplotlib.pyplot as plt

from settings import PHEROMONE_INIT


class EdgeColor(Enum):
    """
    Enum for edge colors
    """
    OUT_PATH = 'blue'
    IN_PATH = 'red'


def set_edge_color(graph, path, color):
    for edge in path:
        graph.edges[edge]['color'] = color


def draw_path(graph, path, color):
    """
    Set color of edges in path, draw graph and reset color of edges

    :param graph: networkx graph
    :param path: list of edges
    :param color: edge tag
    """
    set_edge_color(graph, path, color)
    draw_graph(graph)
    set_edge_color(graph, path, EdgeColor.OUT_PATH.value)


def draw_graph(graph):
    """
    Draw graph using matplotlib and add pause between each frame

    :param graph: networkx graph
    """
    pos = nx.spring_layout(graph, seed=1)
    edges = graph.edges()
    colors = [graph[u][v]['color'] for u, v in edges]

    nx.draw(graph, pos, edge_color=colors, with_labels=True, width=5.0, node_size=1500)
    plt.show()
    plt.pause(0.1)
    plt.close()


def create_graph(file_name):
    """
    Create graph from file

    :param file_name: name of file with graph
    :return: networkx graph, name dict and (start, end) nodes
    """
    G = nx.Graph()

    with open(file_name, 'r') as fin:
        nr_cities = int(fin.readline())
        start_city = fin.readline().split()[0]
        end_city = fin.readline().split()[0]

        city_tuples = [fin.readline().split() for _ in range(nr_cities)]
        # create <name, code> dictionary for cities
        city_name_dict = {
            tup[1]: {
                'id': int(tup[0]),
                'code': tup[2]
            } for tup in city_tuples
        }

        # read edges from file and append to graph
        while line := fin.readline():
            line = line.split()
            G.add_edge(city_name_dict[line[0]]['code'],
                       city_name_dict[line[1]]['code'],
                       color=EdgeColor.OUT_PATH.value,
                       pheromone=PHEROMONE_INIT,
                       probability=0.0,
                       distance=1)

        return G, city_name_dict, (start_city, end_city)
