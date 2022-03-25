#include <iostream>
#include <fstream>
#include <vector>
#include <queue>
#include <stack>
#include <set>
#include <map>
#include <algorithm>
#include <iterator>

using namespace std;

vector<int> operator+(const vector<int>& a, const vector<int>& b) {
    vector<int> c;
    copy(a.begin(), a.end(), back_inserter(c));
    copy(b.begin(), b.end(), back_inserter(c));
    return c;
}

vector<int> getPath(const int& curr, const vector<int>& parent) {
    vector<int> path;
    int curr_node = curr;
    while (curr_node != -1) {
        path.push_back(curr_node);
        curr_node = parent[curr_node];
    }
    reverse(path.begin(), path.end());
    return path;
}

pair<int, vector<int>> runBS(
        const vector<set<int>>& graph,
        const int& start,
        const int& end) {
    vector<int> visited(graph.size(), 0),
                parentBfs(graph.size(), -1),
                parentDfs(graph.size(), -1);
    int distinctCities = 0,
        src = start,
        dest = end,
        mid = -1;

    queue<int> q;
    q.push(src);
    visited[src] = 1;
    ++distinctCities;

    stack<int> st;
    st.push(dest);
    visited[dest] = 2;
    ++distinctCities;

    while (!q.empty() && !st.empty()) {
        /// BFS
        int u = q.front();
        q.pop();

        for (auto v : graph[u]) {
            if (visited[v] == 0) {
                visited[v] = 1;
                parentBfs[v] = u;
                q.push(v);
                ++distinctCities;
            } else if (visited[v] == 2) {
                mid = v;
                parentBfs[v] = u;
                goto compute_path;
            }
        }

        /// DFS
        u = st.top();
        st.pop();

        for (auto v : graph[u]) {
            if (visited[v] == 0) {
                visited[v] = 2;
                parentDfs[v] = u;
                st.push(v);
                ++distinctCities;
            } else if (visited[v] == 1) {
                mid = v;
                parentDfs[v] = u;
                goto compute_path;
            }
        }
    }

    compute_path:
    vector<int> pathBfs = getPath(mid, parentBfs), /// from src to mid
                pathDfs = getPath(mid, parentDfs); /// from dest to mid

    pathDfs.pop_back(); /// remove mid
    reverse(pathDfs.begin(), pathDfs.end());

    return make_pair(distinctCities, pathBfs + pathDfs);
}

int main() {
    ifstream fin("input.txt");

    /// Init graph and cities map
    map<string, int> citiesToInt;
    map<int, string> intToCities;
    int n;
    fin >> n;
    vector<set<int>> graph(n + 1);
    for (int i = 1; i <= n; i++) {
        string city;
        fin >> city;
        citiesToInt[city] = i;
        intToCities[i] = city;
    }
    while (!fin.eof()) {
        string city1, city2;
        fin >> city1 >> city2;
        graph[citiesToInt[city1]].insert(citiesToInt[city2]);
        graph[citiesToInt[city2]].insert(citiesToInt[city1]);
    }
    fin.close();

    /// Run Bidirectional Search
    string start, end;
    cout << "Enter start city:\n";
    cin >> start;
    cout << "Enter end city:\n";
    cin >> end;

    auto [distinctCities, path] = runBS(graph, citiesToInt[start], citiesToInt[end]);
    cout << "Number of distinct cities traversed: " << distinctCities << endl;

    cout << "Path: ";
    transform(path.begin(), path.end(), ostream_iterator<string>(cout, " "),
            [&intToCities](int i) { return intToCities[i]; });

    return 0;
}
