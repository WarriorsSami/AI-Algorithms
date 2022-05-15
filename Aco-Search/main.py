from graph import draw_graph, G, city_name_dict, set_edge_color, EdgeColor

draw_graph(G)

trail = [(city_name_dict['Arad']['code'], city_name_dict['Oradea']['code']),
         (city_name_dict['Oradea']['code'], city_name_dict['Satu-Mare']['code']),
         (city_name_dict['Arad']['code'], city_name_dict['Timisoara']['code']),
         (city_name_dict['Timisoara']['code'], city_name_dict['Alba-Iulia']['code']),
         (city_name_dict['Alba-Iulia']['code'], city_name_dict['Sibiu']['code']),
         (city_name_dict['Sibiu']['code'], city_name_dict['Brasov']['code']),
         (city_name_dict['Brasov']['code'], city_name_dict['Bucuresti']['code']),
         (city_name_dict['Bucuresti']['code'], city_name_dict['Braila']['code']),
         (city_name_dict['Braila']['code'], city_name_dict['Tulcea']['code'])]
set_edge_color(G, trail, EdgeColor.IN_PATH.value)

draw_graph(G)
