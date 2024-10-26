# Object Reactive Animation Generation Using Reinforcement Learning Based on Motion Blending Data <br> (모션 블렌딩 데이터를 활용한 강화학습 기반 객체 반응형 애니메이션 생성)
![title image](.\docs\모방학습프레임연속촬영20frame_6.png)
### [Paper Link]() / [Journal Link](https://www.earticle.net/Article/A445526)
<!-- (결과 gif 2~4개 미리보기용 넣기) -->
<p style="display: flex; justify-content: space-around;">
  <img src="./docs/using-imitate-motion-blending-method.gif" alt="" width="35%" height="20%" />
  <img src="./docs/change-objects/cylinder horizon.gif" alt="" width="35%" height="20%" />
  <img src="./docs/change-objects/plane.gif" alt="" width="35%" height="20%" />
</p>

## 개요
![Large-ScaleTraning](.\docs\Large-ScaleTraining.gif)
###### adove: [ASE: Large-Scale Reusable Adversarial Skill Embeddings for Physically Simulated Characters]
위 그림은 20,000,000 번의 강화학습을 수행한 모습  
2천만번이나 학습했지만 서있는 것 조차 어렵다.

이처럼 물리 기반 시뮬레이션은 학습해야할 양이 너~무 많다.  
더 적은 학습량으로 만족할 만한 애니메이션을 생성하는 방법 연구!

## 아이디어
<!-- ![Architecture](.\docs\Architecture.jpeg) -->
<p align="center">
  <img src="./docs/Architecture.jpeg" alt="" width="80%" />
</p>
(아키텍처 잘라서 설명 넣기)

## 실험 환경
Unity Engine  
[unity ml-agents](https://github.com/Unity-Technologies/ml-agents)

### input data
from mixamo  
6 kind of animation

<p style="display: flex; justify-content: space-around;">
  <img src="./docs/input-anim/pistol walk.gif" alt="pistol walk" width="30%" />
  <img src="./docs/input-anim/standing aim walk forward.gif" alt="standing aim walk forward" width="30%" />
  <img src="./docs/input-anim/walk.gif" alt="standing aim walk forward" width="30%" />
</p>
<p style="display: flex; justify-content: space-around;">
  <img src="./docs/input-anim/zombie walking.gif" alt="" width="30%" />
  <img src="./docs/input-anim/scary clown walk.gif" alt="" width="30%" />
  <img src="./docs/input-anim/standing walk forward.gif" alt="" width="30%" />
</p>

## 문제상황 1
### 손으로 물체를 짚는게 자연스럽지 않다.
(사진 넣기)

## 문제상황 2
### 움직임이 연속적이지 않고 로봇처럼 끊긴다.
(사진 넣기)

## 결과
(사진 넣기)