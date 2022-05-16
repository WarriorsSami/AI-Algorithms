from graph import EdgeColor, draw_path
import numpy as np

from settings import TAUETA_MIN, TAUETA_MAX, PHERO_MAX, PHERO_MIN, FRAME_RATE


class Ant:
    """
    Ant class for handling individual ant behavior
    """

    def __init__(self, start_node, end_node):
        """
        Initialize the ant

        :param start_node: start vertex
        :param end_node: last vertex
        """
        self.start_node = start_node
        self.current_node = start_node
        self.end_node = end_node
        self.tabu_list = []
        self.remember(start_node)
        self.reach_end = False

    def remember(self, node):
        """
        Remember the last node visited and add it to the tabu list

        :param node: node to remember
        """
        self.tabu_list.append(node)

    def forget(self, node):
        """
        Remove a node from the tabu list and update the current node

        :param node: node to forget
        """
        if self.tabu_list[-1] == node:
            self.current_node = self.tabu_list[-2]
        self.tabu_list.remove(node)

    def move(self, node):
        """
        Move to a new node, remember it and update the current node

        :param node: node to move to
        """
        self.current_node = node
        self.remember(node)

    def is_final_node(self):
        """
        Check if the ant is at the end node
        """
        if self.current_node == self.end_node:
            self.reach_end = True

    def enable_start_new_path(self):
        """
        Enable the ant to start a new path
        """
        self.reach_end = False

    def reset_tabu_list(self):
        """
        Reset the tabu list and keep only the start node
        """
        self.tabu_list[1:] = []
        self.current_node = self.start_node


def convert_path_to_edges(path):
    """
    Convert a path to a list of edges

    :param path: list of nodes
    :return: list of edges
    """
    edges = []
    for i in range(len(path) - 1):
        edges.append((path[i], path[i + 1]))
    return edges


def normalize(value, min_value, max_value):
    """
    Normalize a value between a min and max value

    :param value: number to normalize
    :param min_value: lower bound
    :param max_value: upper bound
    :return: normalized value
    """
    if value < min_value:
        return min_value
    elif value > max_value:
        return max_value
    return value


def remove_cycles(path):
    """
    Remove cycles from a path by deleting node duplicates and intermediate nodes

    :param path: list of nodes
    :return: path without cycles
    """

    def get_coincidence_indices(store, item):
        """
        Return the indices of the coincidence of an item in a list

        :param store: list of items
        :param item: item to find
        :return: list of indices of coincidence
        """
        return [i for i, x in enumerate(store) if x == item]

    for node in path:
        indices = get_coincidence_indices(path, node)
        indices.reverse()
        for i, coincidence in enumerate(indices):
            if i != len(indices) - 1:
                path[indices[i + 1]:coincidence] = []

    return path


class AntColony:
    """
    Ant colony class for handling the ant colony behavior
    """

    def __init__(self, graph, start_node, end_node, ants_count, alpha, beta, rho, q, max_iterations):
        """
        Initialize the ant colony

        :param graph: the graph to use
        :param start_node: source vertex
        :param end_node: destination vertex
        :param ants_count: number of ants
        :param alpha: power of the pheromone
        :param beta: power of the heuristic
        :param rho: coefficient of the pheromone evaporation
        :param q: coefficient of the pheromone addition
        :param max_iterations: number of iterations
        """
        self.graph = graph
        self.start_node = start_node
        self.end_node = end_node
        self.ants_count = ants_count
        self.alpha = alpha
        self.beta = beta
        self.rho = rho
        self.q = q
        self.max_iterations = max_iterations

        self.ants = self.init_ants()
        self.paths = []
        self.best_result = []

    def init_ants(self):
        """
        Initialize the ant colony
        :return: list of newly created ants
        """
        ants = []
        for i in range(self.ants_count):
            ants.append(Ant(self.start_node, self.end_node))
        return ants

    def update_pheromones(self):
        """
        Update the pheromones of the edges using the following formula:

        if edge in path:
        pheromone[edge] = (1 - rho) * pheromone[edge] + q / path_length

        if edge not in path:
        pheromone[edge] = (1 - rho) * pheromone[edge]
        """
        for path in self.paths:
            for node in path:
                for neigh in self.graph[node]:
                    decrease = self.graph[node][neigh]['pheromone'] * (1 - self.rho)
                    increase = self.q / self.graph[node][neigh]['distance']
                    if neigh in path:
                        self.graph[node][neigh]['pheromone'] = decrease + increase
                    else:
                        self.graph[node][neigh]['pheromone'] = decrease
                    self.graph[node][neigh]['pheromone'] = normalize(self.graph[node][neigh]['pheromone'],
                                                                     PHERO_MIN, PHERO_MAX)

    def get_probability(self, current_node, next_node):
        """
        Calculate the probability contribution of moving to the next node

        :param current_node:
        :param next_node:
        :return:
        """
        return self.graph[current_node][next_node]['pheromone'] ** self.alpha \
               * (1 / self.graph[current_node][next_node]['distance']) ** self.beta

    def choose_next_node(self, current_node):
        """
        Choose the next node to move to using the Monte Carlo method

        :param current_node: the current node for the ant
        :return: next node
        """
        total_sum = 0
        for neigh in self.graph[current_node]:
            self.graph[current_node][neigh]['probability'] = normalize(self.get_probability(current_node, neigh),
                                                                       TAUETA_MIN, TAUETA_MAX)
            total_sum += self.graph[current_node][neigh]['probability']

        neigh_list = []
        cumulative_probs = [0] * (len(self.graph[current_node]) + 1)
        for idx, neigh in enumerate(self.graph[current_node]):
            neigh_list.append(neigh)
            self.graph[current_node][neigh]['probability'] /= total_sum
            cumulative_probs[idx + 1] = cumulative_probs[idx] + self.graph[current_node][neigh]['probability']

        for neigh in self.graph[current_node]:
            self.graph[current_node][neigh]['probability'] = 0.0

        rand_num = np.random.random()
        for idx in range(len(cumulative_probs) - 1):
            if cumulative_probs[idx] <= rand_num < cumulative_probs[idx + 1]:
                return neigh_list[idx]

        raise Exception('Unable to find next node')

    def sort_paths(self):
        self.paths.sort(key=len)

    def clear_paths(self):
        self.paths = []

    def add_to_paths(self, path):
        self.paths.append(path)

    def run(self):
        """
         The main loop of the ACO algorithm
        """
        # for each iteration
        for i in range(self.max_iterations):
            # for each ant
            for ant in self.ants:
                # till the end node is not reached
                while not ant.reach_end:
                    # choose the next node using the probability distribution and the pheromone values
                    next_node = self.choose_next_node(ant.current_node)
                    # add it to the tabu list of the ant
                    ant.move(next_node)
                    # check if the ant has reached the end node
                    ant.is_final_node()

                # add the path to the paths list after removing any existing cycle
                self.add_to_paths(remove_cycles(list(ant.tabu_list)))
                # clear the tabu list of the ant
                ant.reset_tabu_list()
                # enable the ant to move again
                ant.enable_start_new_path()

            # sort the paths based on the length of the path
            self.sort_paths()
            # update the pheromone values
            self.update_pheromones()
            # choose the best path
            self.best_result = self.paths[0] \
                if len(self.paths[0]) < len(self.best_result) or len(self.best_result) == 0 \
                else self.best_result
            # reset path list
            self.clear_paths()

            print(f'Iteration {i} with len: {len(self.best_result)}')
            if i % FRAME_RATE == 0:
                # draw the graph for the current best path
                draw_path(self.graph, convert_path_to_edges(self.best_result), EdgeColor.IN_PATH.value)

        return self.best_result
