# 🤖 Gemini Code Assistant System Context
이 문서는 '푸드트럭 타이쿤(Food Truck Tycoon)' 프로젝트의 컨텍스트와 코딩 가이드라인을 AI Assistant에게 제공하기 위한 파일입니다. 코드를 제안하거나 리팩토링할 때 아래의 규칙을 반드시 엄격하게 준수하십시오.

## 📌 1. Project Overview
* **프로젝트명:** Food Truck Tycoon (가제)
* **플랫폼:** PC (Primary) / Mobile
* **엔진 및 언어:** Unity 2D, C#
* **장르:** 2D 요리 타이쿤 + 경영 시뮬레이션
* **핵심 메커니즘:** 재료 시세 변동 장터, 레시피 연금술(순서 기반 층층 생성), 타일맵 기반 자동화/경영, 타겟 맞춤형 시식 피드백 및 VIP 웨이팅 시스템.
* **배경:** 수원(Suwon)의 랜드마크와 건축 요소를 활용한 아트워크 및 기획.

## ⚠️ 2. CRITICAL Coding Guidelines (절대 준수)
코드를 작성하거나 수정할 때 다음 사항을 최우선으로 고려하십시오.

1. **LINQ 사용 금지 (No LINQ):**
   * 성능 저하 및 가비지 컬렉션(GC) 할당을 방지하기 위해 `System.Linq` 네임스페이스 및 관련 확장 메서드(예: `.Where`, `.Select`, `.ToList()` 등)의 사용을 엄격히 금지합니다.
   * 모든 컬렉션 탐색 및 조작은 전통적인 `for` 또는 `foreach` 루프를 사용하십시오.
2. **저장소 및 플랫폼 최적화 (PC & JSON):**
   * 이 게임의 주 플랫폼은 PC입니다.
   * `PlayerPrefs`의 사용을 피하고, 환경설정(사운드, 해상도 등) 및 게임 세이브 데이터는 반드시 `Application.persistentDataPath`를 활용한 **JSON 직렬화/역직렬화** 방식으로 구현하십시오.
3. **그리드 및 좌표계 규칙 (Map Bounds):**
   * 농사/타일 맵 등의 그리드 좌표계에서 **음수 좌표는 절대 허용되지 않습니다.**
   * 맵의 최대 한계선(Bounds)은 `(0, 0)` 부터 `(14, 8)` 까지(총 15x9 그리드)로 고정하여 연산하십시오.
4. **UI 슬라이더 계산 규칙:**
   * 커스텀 슬라이더 구현 시, `handleArea`와 `fillMask`의 Width 값이 다릅니다.
   * 단순한 0~1 사이의 비율(Ratio) 계산이 아닌, **실제 픽셀 기반(Pixel-based)의 계산식**을 적용하여 UI 시각 오류를 방지하십시오.
5. **메모리 및 렌더링 최적화:**
   * **GC 최소화:** Update 루프 내에서의 메모리 할당(`new`, 문자열 조작 등)을 금지합니다.
   * **셰이더 최적화:** 2D 바람에 흔들리는 효과(Wind sway) 등의 HLSL 셰이더 작성 시, 별도의 마스크 텍스처를 샘플링하지 말고 **버텍스 컬러 알파(Vertex color alpha)를 마스크로 활용**하여 렌더링 효율을 높이십시오. 다수 객체의 배칭(Batching)이 깨지지 않도록 주의하십시오.

## 🧠 3. Advanced Systems Context
* **LLM 제어 에이전트 (AI Agent):**
  * 게임 내 NPC나 자동화 에이전트는 자연어(LLM 프롬프트)를 기반으로 JSON 커맨드를 파싱하여 행동(이동, 당근/체리 심기, 수확 등)합니다. 관련된 스크립트는 이 JSON 커맨드 구조와 매핑되도록 모듈화되어야 합니다.
* **절차적 생성 (Procedural Generation):**
  * 맵이나 건물(방, 벽 등) 생성 시 `Tilemap` 시스템과 `Instantiate`를 활용한 랜덤 룸 생성 및 공간 부착 로직을 기반으로 합니다.
* **모듈형 아키텍처:**
  * 환경설정 등은 멀티 컨트롤러 시스템을 구축하여 확장 가능하도록 작성하십시오.

## 📂 4. Directory Structure Mapping
AI는 다음 경로를 참조하여 적절한 위치에 코드를 제안해야 합니다.

* `Assets/Scripts/Game/Cook/`: 요리 제작, 냄비 로직(`CookingPot.cs`), 레시피 연금술, 재료 객체.
* `Assets/Scripts/Game/AI/`: 손님 스폰, 시식 피드백, 인내심 UI 로직, 외형/데이터 DB.
* `Assets/Scripts/Game/Player/MiniGame/`: 재료 가공용 터치 및 타이밍 미니게임.
* `Assets/Scripts/Game/Market/`: 새벽 시장 및 일반 상점의 시세 변동 로직.
* `Assets/ScriptableObjects/`: 손님 외형, 요리(BreadTower, Sandwich 등), 재료(Bread 등)의 데이터 중심 아키텍처 (Scriptable Object 활용).