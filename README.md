# AI-Empowered Digital Twin Navigation System for NVIDIA JetBot

## Project Description
This project develops a Digital Twin navigation system for the NVIDIA JetBot, integrating Unity 3D to simulate real-world scenarios with a focus on obstacle detection, avoidance, and guidance. By creating a virtual replica of the JetBot in Unity 3D, the system allows for the safe and efficient testing of navigation algorithms under varied conditions, including complex obstacle-rich environments. The simulation facilitates the study and enhancement of the JetBot's response mechanisms, crucial for reliable obstacle detection and avoidance. A bidirectional communication setup between the virtual and physical JetBot ensures that insights and adaptations from the simulated trials directly inform real-world operations. This innovative approach speeds up the development of effective navigation strategies, reduces the risks and costs of physical testing, and advances the field of autonomous robotic navigation.
## 3D Environment
This process involves using the iPad Pro's LiDAR camera for environment scanning and converting the data into a 3D model, which is then imported into Unity for texture application and boundary setup.

### Steps

1. **Environment Scanning and 3D Model Conversion**:
   - The iPad Pro's LiDAR camera is used to scan the environment, capturing detailed spatial information.
   - The scanned data is then converted into a 3D model format that is compatible with Unity, ensuring accurate representation of the physical space.

2. **Importing into Unity and Detailing**:
   - The 3D model is imported into the Unity engine for further development.
   - Textures are applied to the model to replicate the real-world appearance, and boundaries are set up within Unity to match the physical properties of the environment.

![3D Scan](https://github.com/duyhho/Digital-Twins/blob/main/Media/3d%20scan.gif)

## Learning Components

### Reinforcement Learning with PPO and Curiosity (via Unity ML-Agents)
![Reinforcement Learning GIF](https://github.com/duyhho/Digital-Twins/blob/main/Media/RL.gif)

**ML Agents** in Unity are designed for environments where the layout is unknown or dynamically changes. They are ideal for scenarios where agents need to explore and adapt in real-time.

- **Dynamic Exploration**: Utilizes ray sensors for real-time environment understanding.
- **Objective-Driven**: Trains agents to identify and interact with specific targets or goals.
- **Adaptive Learning**: Employs machine learning techniques, including reinforcement learning, for continuous adaptation.
### Unity AI Navigation
**Unity AI Navigation** is optimized for known environments, using a pre-defined Navigation Mesh (NavMesh) for pathfinding and navigation.

- **NavMesh-Based**: Relies on a pre-built representation of the environment.
- **Obstacle Navigation**: Efficiently navigates around both static and dynamic obstacles.
- **Pathfinding**: Implements algorithms for optimal path determination in a known layout.
- 
#### AI Navigation to Destination (target table highlighted in green)

![AI Navigation to Destination](https://github.com/duyhho/Digital-Twins/blob/main/Media/ai-nav-1.gif)

#### AI Navigation to Destination with Static Obstacles (target table highlighted in green)
Obstacles: Trash and Small Objects on the floor

![AI Navigation to Destination with Obstacles](https://github.com/duyhho/Digital-Twins/blob/main/Media/ai-nav-2.gif)
#### AI Navigation to Destination with Static + Dynamic Obstacles (target table highlighted in green)
Static: Trash
Dynamic/Moving: People

![AI Navigation to Destination with Obstacles](https://github.com/duyhho/Digital-Twins/blob/main/Media/ai-nav-people.gif)

**ML Agents** are best for unpredictable environments requiring exploration and learning, while **Unity AI Navigation** excels in static or minimally changing settings, focusing on path optimization and obstacle avoidance.

### Demo Video
ML Agents and Unity AI Navigation

[![ML Agents and Unity AI Navigation](https://img.youtube.com/vi/t0jWeVxQrtI/0.jpg)](https://www.youtube.com/watch?v=t0jWeVxQrtI)


# Digital Twin Navigation System for NVIDIA JetBot 
This section focuses on deploying the trained models to the real JetBot via real-time communication and commands.

[![Digital Twin Navigation System for NVIDIA JetBot](https://img.youtube.com/vi/WV505kwxo0U/0.jpg)](https://www.youtube.com/watch?v=WV505kwxo0U)

### Description
- **Real-World Simulation**: Utilizes Unity 3D for creating realistic scenarios for testing and development.
- **Obstacle Detection and Avoidance**: Focuses on improving the JetBot's ability to navigate through challenging environments.
- **Bidirectional Communication**: Ensures that the virtual and real-world JetBots can exchange data and learnings in real-time.
- **Risk Reduction**: Minimizes the risks and costs associated with physical testing.

