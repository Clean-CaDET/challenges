# Supplementary Maintainability Challenges
This repository hosts several maintainability challenges that can be processed by the Clean CaDET platform. It is referenced in the paper _An Intelligent Tutoring System to Support Code Maintainability Skill Development_, submitted to _IEEE Transactions on Learning Technologies_ for revision.

The repository has three maintainability challenges. Each consists of the starting code (in the .CS file) and a README.md that includes:

1. The description of the challenge,
2. A description of the problem solving approach for solving the challenge, which ends with a possible solution,
3. The used maintainability issue detectors, and
4. A discussion of the limitations of the concrete maintainability issue detectors to constrain the solution space.

Apart from the challenge description, students wouldn't receive the solution or maintainability issue detectors specification, unless the goal was to present a worked example.

The challenges are grouped around three distinct code maintainability units. They are, in order of complexity:

1. Challenge for removing noise words to produce meaningful names ([link](https://github.com/Clean-CaDET/challenges/tree/master/Naming)),
2. Challenge for extracting complex logic to produce maintainable functions ([link](https://github.com/Clean-CaDET/challenges/tree/master/Methods)), and
3. Challenge for creating a class with a single responsibility to produce maintainable classes ([link](https://github.com/Clean-CaDET/challenges/tree/master/Classes)).

## Code Maintainability ITS
What follows is a list of useful links for our ITS code and documentation repositories.

| Resource	| Summary	| Link
| --------- | ------- | ------- |
| ITS back-end source code	| A repository hosting the source code of the ITS server application that implements our conceptual model.	| [Link](https://github.com/Clean-CaDET/tutor/tree/v2.9.0) |
| Web front-end source code	| A repository hosting the source code of the web UI that supports most of the learner and instructor features.	| [Link](https://github.com/Clean-CaDET/platform-tutor-ui-web/tree/v2.9.0) |
| VSC Plugin source code	| A repository hosting the source code of our learning instrument for the maintainability challenge assessment item.	| [Link](https://github.com/Clean-CaDET/platform-plugin-vscode/tree/feat-english) |
| VSC Plugin	| The learning instrument that can be loaded into VSC.	| [Link](https://marketplace.visualstudio.com/items?itemName=CleanCaDET.clean-cadet-en) |
| Installation guidelines	| Guidelines for setting up a local instance of our ITS, including the back-end and front-end applications.	| [Link](https://github.com/Clean-CaDET/tutor/tree/v2.9.0/SETUP.md) |
| General documentation	| A collection of wiki pages explaining our ITS design and supported features.	| [Link](https://github.com/Clean-CaDET/tutor/wiki) |
