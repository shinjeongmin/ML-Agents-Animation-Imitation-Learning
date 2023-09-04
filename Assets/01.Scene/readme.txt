04.AnimationKeyFrameTxtExport

이 씬은 Robot Kyle To Export 오브젝트에의 animator에서
각 프레임별로 각각의 BoneTransform들의 local position과 local rotation을 뽑아서 json 형식의 txt파일로 저장한다.
Robot Kyle To Import는 파일로 export한 애니메이션 정보들을 받아서 parsing한 후 적용하도록 형태를 만든다.
Import 적용할 때 offset slider를 조정하여 프레임별 동작을 보도록 하거나,
nextFrame 함수 또는 getFrame(int) 함수를 실행하면 해당 프레임의 tranform joint값들을 Robot Kyle To Import에 적용할 수 있도록 한다.

