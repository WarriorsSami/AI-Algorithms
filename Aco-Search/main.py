from ant_colony import AntColony, convert_path_to_edges
from graph import create_graph, draw_path, EdgeColor
from settings import ALPHA, BETA, RHO, Q, MAX_TIME, NO_ANTS

try:
    G, city_name_dict, (start_city, end_city) = create_graph('data\\cities.txt')

    colony = AntColony(G, city_name_dict[start_city]['code'], city_name_dict[end_city]['code'],
                       NO_ANTS, ALPHA, BETA, RHO, Q, MAX_TIME)

    path = colony.run()
    # convert path codes to path names
    city_name_from_code = {v['code']: k for k, v in city_name_dict.items()}
    path_name = [city_name_from_code[i] for i in path]
    print(f"Best path: {path_name} with length: {len(path)}")

    # draw path using edges
    path_from_edges = convert_path_to_edges(path)
    draw_path(G, path_from_edges, EdgeColor.IN_PATH.value)
except Exception as e:
    print(e)
