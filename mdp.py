# AIMA Python file: mdp.py
# http://aima.cs.berkeley.edu/python/mdp.html
"""Markov Decision Processes (Chapter 17)
First we define an MDP, and the special case of a GridMDP, in which
states are laid out in a 2-dimensional grid.  We also represent a policy
as a dictionary of {state:action} pairs, and a Utility function as a
dictionary of {state:number} pairs.  We then define the value_iteration
and policy_iteration algorithms."""

from utils import *

"""
the grid is in [row, col]
In the future, everything, including action and state, is all in [row,col]
and the row 0 is on the top, like an 2D array [[1,2,3],[4,5,6]]
"""

class MDP:
    """A Markov Decision Process, defined by an initial state, transition model,
    and reward function. We also keep track of a gamma value, for use by
    algorithms. The transition model is represented somewhat differently from
    the text.  Instead of T(s, a, s') being probability number for each
    state/action/state triplet, we instead have T(s, a) return a list of (p, s')
    pairs.  We also keep track of the possible states, terminal states, and
    actions for each state. [page 615]"""

    # XXX: gamma = discount
    # XXX: terminal = the end state
    #      (when we get to the end state, whatever reward we have, we will stop moving)
    # TODO: now R is a function of s, but it could be a function of (s,a)
    def __init__(self, init, actlist, terminals, gamma=.9):
        update(self, init=init, actlist=actlist, terminals=terminals,
               gamma=gamma, states=set(), reward={})

    def R(self, state):
        "Return a numeric reward for this state."
        return self.reward[state]

    def T(state, action):
        """Transition model.  From a state and an action, return a list
        of (result-state, probability) pairs."""
        abstract

    def actions(self, state):
        """Set of actions that can be performed in this state.  By default, a
        fixed list of actions, except for terminal states. Override this
        method if you need to specialize by state."""
        if state in self.terminals:
            return [None]
        else:
            return self.actlist

class GridMDP(MDP):
    """A two-dimensional grid MDP, as in [Figure 17.1].  All you have to do is
    specify the grid as a list of lists of rewards; use None for an obstacle
    (unreachable state).  Also, you should specify the terminal states.
    An action is an (x, y) unit vector; e.g. (1, 0) means move east."""

    def __init__(self, grid, terminals, init=(0, 0), gamma=.9):
        MDP.__init__(self, init, actlist=orientations,
                     terminals=terminals, gamma=gamma)
        update(self, grid=grid, rows=len(grid), cols=len(grid[0]))
        # store rewards in a map
        for i in range(self.rows):
            for j in range(self.cols):
                # reward = {(0, 0): -0.04,}   
                self.reward[i, j] = grid[i][j]
                if grid[i][j] is not None:
                    # if the state is reachable, add it
                    self.states.add((i, j))

    def T(self, state, action):
        # [(probability, end_state),]
        if action == None:
            # XXX: should be 1.0
            return [(0.0, state)]
        else:
            # return [(0.8, self.go(state, action)),
                    # (0.1, self.go(state, turn_right(action))),
                    # (0.1, self.go(state, turn_left(action)))]
            # XXX: no uncertainty
            return [(1., self.go(state, action))]


    def go(self, state, direction):
        "Return the state that results from going in this direction."
        state1 = vector_add(state, direction)
        # if the next state is not valid, then don't go there 
        return if_(state1 in self.states, state1, state)

    # transform a grid_map into a arrow_grid_map based on the policy in the mapping
    def to_grid(self, mapping):
        """Convert a mapping from (i, j) to v into a [[..., v, ...]] grid."""
        return [[mapping.get((i,j), None) for j in range(self.cols)] 
                for i in range(self.rows)]

    # transform from a policy {[state_i,state_j]:[action_i,action_j]}
    # to a path in the grid {[i,j]:'>'}
    def to_arrows(self, policy):
        chars = {(0,1):'>', (-1,0):'^', (0,-1):'<', (1,0):'v', None: '.'}
        return self.to_grid(dict([(s, chars[a]) for (s, a) in policy.items()]))

# use value_iteration to learn the utility function for each position in grid
def value_iteration(mdp, epsilon=0.001):
    "Solving an MDP by value iteration. [Fig. 17.4]"
    # value of each state
    U1 = dict([(s, 0) for s in mdp.states])
    R, T, gamma = mdp.R, mdp.T, mdp.gamma
    while True:
        U = U1.copy()
        delta = 0
        for s in mdp.states:
            U1[s] = R(s) + gamma * max([sum([p * U[s1] for (p, s1) in T(s, a)])
                                        for a in mdp.actions(s)])
            # based on Peter Abbeel class
            # U1[s] = R(s) + gamma * max([sum([p * (R(s1)+gamma*U[s1]) for (p, s1) in T(s, a)])
                                        # for a in mdp.actions(s)])
            delta = max(delta, abs(U1[s] - U[s]))
        # when the max change is very small (convergence), then we should stop
        # print epsilon * (1 - gamma) / gamma = 1*10^-5
        if delta < epsilon * (1 - gamma) / gamma:
             return U

# iterate through the grid and get the best solution based on the utility function
"""
pi = {state: action,}
pi =
{(0, 0): (0, 1),
 (0, 1): (0, 1),
 (0, 2): (1, 0),
 (1, 0): (1, 0),
 (1, 2): (1, 0),
 (2, 0): (0, 1),
 (2, 1): (0, 1),
 (2, 2): (1, 0),
 (3, 0): (-1, 0),
 (3, 1): None,
 (3, 2): None}
 """
def best_policy(mdp, U):
    """Given an MDP and a utility function U, determine the best policy,
    as a mapping from state to action. (Equation 17.4)"""
    pi = {}
    for s in mdp.states:
        pi[s] = argmax(mdp.actions(s), lambda a:expected_utility(a, s, U, mdp))
    return pi

def expected_utility(a, s, U, mdp):
    "The expected utility of doing a in state s, according to the MDP and U."
    # return sum([p * U[s1] for (p, s1) in mdp.T(s, a)])
    # https://www.cs.cmu.edu/afs/cs/project/jair/pub/volume4/kaelbling96a-html/node20.html
    return mdp.R(s)+sum([p * U[s1] for (p, s1) in mdp.T(s, a)])


def policy_iteration(mdp, k):
    "Solve an MDP by policy iteration [Fig. 17.7]"
    U = dict([(s, 0) for s in mdp.states])
    # random choose action for each state
    # TODO: a new init policy
    pi = dict([(s, random.choice(mdp.actions(s))) for s in mdp.states])
    while True:
        # TODO: k == 1???
        U = policy_evaluation(pi, U, mdp, k)
        unchanged = True
        for s in mdp.states:
            a = argmax(mdp.actions(s), lambda a: expected_utility(a,s,U,mdp))
            if a != pi[s]:
                pi[s] = a
                unchanged = False
        if unchanged:
            return pi

# update the utility for k times based on the policy
def policy_evaluation(pi, U, mdp, k=20):
    """Return an updated utility mapping U from each state in the MDP to its
    utility, using an approximation (modified policy iteration)."""
    R, T, gamma = mdp.R, mdp.T, mdp.gamma
    for i in range(k):
        for s in mdp.states:
            U[s] = R(s) + gamma * sum([p * U[s] for (p, s1) in T(s, pi[s])])
            # based on Peter Abbeel class
            # U[s] = sum([p * (R(s) + gamma*U[s]) for (p, s1) in T(s, pi[s])])
    return U


def test():
    # XXX: grid_map, terminals, init, are all in [row,col]
    """
    grid_map = 
    0,2 1,2 2,2 3,2
    0,1 1,1 2,1 3,1
    0,0 1,0 2,0 3,0
    """
    grid_map = GridMDP([[-0.04, -0.04, -0.04, +1],
                         [-0.04, None,  -0.04, -1],
                         [-0.04, -0.04, -0.04, -0.04]],
                        terminals=[(0, 3), (1, 3)])
    U = value_iteration(grid_map)
    pi = best_policy(grid_map, U)
    print 'value iteration:'
    print pi
    print grid_map.to_arrows(pi) 

    pi = policy_iteration(grid_map,1)
    print 'policy iteration:'
    print pi
    print grid_map.to_arrows(pi) 



# test()


