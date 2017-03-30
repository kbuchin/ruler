# Art Gallery
## Controls

Welcome eminence,

Please illuminate the following dungeons using as few torches as possible.

Good luck!

Controls:
- Click to place a torch.
- Drag a torch to move it.
- Drag and release a torch outside the dungeon to remove it.

## Background

The problem that you solve in this game is known as the "art gallery problem".

The lighting area of each torch is calculated via a sweepcircle algorithm. We then use the Weiler-Atherron algorithm to remove duplicate parts from the sight areas and finally check if the sum of the sight areas is equal to the area of the dungeon.


# Kings Taxes
## MST

Welcome eminence,

You are an advisor for a very cheap king.  He has a kingdom with lots of villages and castles. Of course this king wants to charge taxes to all his subjects. In order to do this you need to advise the king on how to build a road network of minimal length that reaches all settlements.
This problem is known as the Minimum Spanning Tree problem.

Controls:
- Dragging from one settlement to another will create a road.
- Left click on a road to remove it.

## TSpanner

Welcome eminence,

This time the king's subjects won't put up with the kings lousy road networks any longer. They want to be able to travel towards nearby friends and family in a efficient manner. To be more precise, they want to able to travel to every other settlement in the kingdom without covering more than 1.5 times the distance "as the bird flies".
This problem is known as the t-spanner problem. The value of t in this case is 1.5.

Controls:
- Dragging from one settlement to another will create a road.
- Left click on a road to remove it.
- You will be asked to beat the score obtained by a greedy algorithm.
- There is a hint button, clicking it will display the next road the greedy algorithm would place.
- When you beat the score you can either Advance or keep playing for a better highscore.

## TSP

Welcome eminence,

This time the king has another approach. He wants you to design a road network such that his tax collector can visit all settlements in one go and in a tour that is as short as possible.

This problem is known as the Traveling Salesman Problem (TSP).


Controls:
- Dragging from one settlement to another will create a road.
- Left click on a road to remove it.
- Once you beat a certain score you are allowed to advance.
- Try to keep playing to beat the highscore

## Background

For the minimum Spanning Tree game we implemented Prim's algorithm.

In the t-spanner game we let the player win if they beat a greedy algorithm that orders the edges of the complete graph on length and keeps adding the shortest one while the two nodes connected by this edge do not yet have a connection of length 1.5 times the direct distance.

In the Traveling Salesman Problem we challenge the player to find a tour that is shorter than the distance of the tour provided by Christofides algorithm.

# Voronoi
## Controls

Welcome, knights!

You must conquer the land by placing castles in strategic positions. When it's your turn, click anywhere on the screen to place a castle. The player with the most land under their control wins!

When you press C, E or V you can see the underlying data structures (Circles, Delauny triangulation and Voronoi diagram, respectively).

## Background

Given a set of locations (the castles), the Voronoi diagram is the decomposition of the plane into regions such that within a region all points are closest to the same location (castle). The Delaunay triangulation is the graph that we obtain by taking the castles as vertices, and connecting two castles if their Voronoi regions touch. If no four points lie on the same circle, this graph is a triangulation. The triangles in the Delaunay triangulation are characterized by the property that the circumcircle of a triangle contains no points in its interior.

We compute the Delaunay triangulation using a randomized incremental construction, and then construct the Voronoi diagram from the Delaunay triangulation.

# Divide



## Controls
Welcome eminence,

The enemy is planning to attack the kingdom from two sides. Therefore the ruler's army has to be divided into two equally strong units. Divide the army using one straight line.

- Click two soldiers to swap them.
- Drag to make a cut.
- Use Q, W, E and R to toggle solution lines.
- Use A, S and D to display geometric duals.
- Use +, - and the arrow keys to navigate the dual (the yellow faces are feasible).

## Background
This game is based on the ham-sandwich theorem. This theorem states that if there are n measurable objects in an n dimensional space, then there is one (n-1)-dimensional slice that cuts them all in half.

The characters in this game are a random distribution of points. They belong to different categories, corresponding to the different classes (i.e. mage, solder or archer), and are guaranteed to have a solution (i.e. a possible cut exists).

After the initial distribution of points, the algorithm swaps a few points from varying categories around to make the solution harder to find. After that, it is up to the player make the ham-sandwich cut, which might require the player to perform a few swaps first in order to create a composition of three classes that can be separated in half with a single cut.

The short version:

The player’s performance is measured by detecting how many different point sets the player successfully cut in half. For each point set, the algorithm computes the area in which all possible cuts can be made. Placing a cut within this area would successfully divide that particular point set in half. To find the cut(s) that divide(s) the point sets from all categories in half, the algorithm finds the intersection between the areas in which cuts can be placed for each point set. Placing a cut within this intersection area would successfully divide all point sets from different categories in half.

The long version:

The ham-sandwich cut area for each of the different point sets is computed by considering the distribution of each pointset as a primal graph, which can be dualized to obtain a graph containing lines. Dualizing a graph turns points (P_x,P_y) into lines
y = P_x*x-P_y and vice versa. It can be seen that if a point intersects a line in the prime graph, then the dual-line from that point will go trough the dual-point of that line in the dual graph.

Using this property of duality, observe that - for each point set - if all dual-lines intersect at a point, then the dual of that point is a line that goes trough all those points.

Furthermore the duality transform persevere vertical ordering. That is if a line was above and below one point then in the dual it will be a point that is above and below a line.

So to divide a pointset in the primal plane in two equal parts with a line we need to find a point in the dual plane that lies above as many lines as below. All such points form a series of convex polygons in the dual plane. In the following text, these areas are referred to as viable-cut areas.

This problem repeats itself for every pointset (e.g. archers or mages). The underlying algorithm uses this concept to find possible cuts for individual or combined pointsets.

When overlapping the viable cut areas from each point set, the intersection of them is the area in which a cut can be made that successfully divide all sets of objects in half. This area is shown to the player (in a primal version) when he presses Q,W,E or R.

## Victory
Congratulations,

Thanks to your masterful division skills we won!


#Credits
Project Lead: 		Kevin Buchin
Programing: 			Sander Beekhuis and Thom Hurks
Art: 						Willem Sonke and Thom Castermans
Font:						Eagle Lake and Arial

#nonactual vredits
  Project Lead: 		Kevin Buchin
  Programing: 			Sander Beekhuis and Thom Hurks
  Art: 						Willem Sonke and Thom Castermans
  Music: 					Kevin MacLeod ("Oppresive Gloom", "StormFront", "At Launch") and Alexandr Zhelanov
  ("Castlecall")
  Sound Effects: 		LittleRobotSoundFactory
  Font:						Eagle Lake
   and Arial


  ----Official-----
  Stormfront Kevin MacLeod (incompetech.com)
  Oppresive Gloom Kevin MacLeod (incompetech.com)
  At Launch Kevin MacLeod (incompetech.com)
  Alexandr Zhelanov, https://soundcloud.com/alexandr-zhelanov
  Licensed under Creative Commons: By Attribution 3.0 License
   (http://creativecommons.org/licenses/by/3.0/)
