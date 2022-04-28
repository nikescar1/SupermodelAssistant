using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoverFlowCell : QuadCell
{
    public Text gameNameLabel;
    /*
    public void Setup(Texture2D texture, string name)
    {
        renderer.material.mainTexture = texture;
        gameNameLabel.text = name;
    }
    */
    public override void SetData(object data)
    {
        base.SetData(data);

        QuadCellData quadData = data as QuadCellData;

        if (m_material != null)
        {
            if (quadData != null && quadData.Game != null)
            {
                m_material.mainTexture = quadData.Game.gameGroup.flyerFront.texture;
                m_renderer.material = m_material;
            }
        }

        if (quadData != null && quadData.Game != null)
        {
            Messaging.OnGameSelectedCoverFlow(quadData.Game);
        }
    }
}
