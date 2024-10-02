using System.Collections.Generic;
using UnityEngine;

public class ProjectorManager : MonoBehaviour
{
    [System.Serializable]
    public class ProjectorData
    {
        public Projector projector; // 프로젝터 오브젝트
        public Color initialColor;  // 초기 색상
        public Color changedColor;  // 변경할 색상
        public Detection detection; // 감지 시스템
    }

    public List<ProjectorData> projectors = new List<ProjectorData>();

    void Start()
    {
        // 모든 프로젝터 초기화 (크기와 초기 색상 설정)
        foreach (var projectorData in projectors)
        {
            if (projectorData.detection != null)
            {
                SetupProjector(projectorData);  // 초기 설정
            }
        }
    }

    // 초기 프로젝터 설정 (크기 및 초기 색상)
    void SetupProjector(ProjectorData projectorData)
    {
        if (projectorData.projector != null && projectorData.detection != null)
        {
            // 메터리얼을 인스턴스화하여 독립적인 메터리얼을 사용하도록 설정
            projectorData.projector.material = new Material(projectorData.projector.material);

            // Detection의 Range 값의 2배로 프로젝터의 크기를 설정
            float range = projectorData.detection.Range;
            projectorData.projector.orthographicSize = range * 2f;  // 초기 크기 설정
            
            // 초기 색상을 메터리얼에 설정
            projectorData.projector.material.SetColor("_Color", projectorData.initialColor);
        }
    }

    // 모든 프로젝터의 색상을 changedColor로 변경
    public void ChangeAllProjectorsToChangedColor()
    {
        foreach (var projectorData in projectors)
        {
            if (projectorData.projector != null)
            {
                // 색상을 변경할 색상으로 설정
                projectorData.projector.material.SetColor("_Color", projectorData.changedColor);
            }
        }
    }

    // 모든 프로젝터의 색상을 initialColor로 변경
    public void ChangeAllProjectorsToInitialColor()
    {
        foreach (var projectorData in projectors)
        {
            if (projectorData.projector != null)
            {
                // 색상을 초기 색상으로 설정
                projectorData.projector.material.SetColor("_Color", projectorData.initialColor);
            }
        }
    }
}
