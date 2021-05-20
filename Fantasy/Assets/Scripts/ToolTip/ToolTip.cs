using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    public static string text;
	public static bool isUI;

	public Color BGColor = Color.white;
	public Color textColor = Color.black;
	public enum ProjectMode {Tooltip3D = 0, Tooltip2D = 1};
	public ProjectMode tooltipMode = ProjectMode.Tooltip3D;
	public int fontSize = 14; // размер шрифта
	public int maxWidth = 250; // максимальная ширина Tooltip
	public int border = 10; // ширина обводки
	public RectTransform box;
	public RectTransform arrow;
	public Text boxText;
	public Camera _camera;
	public float speed = 10; // скорость плавного затухания и проявления

	private Image[] img;
	private Color BGColorFade;
	private Color textColorFade;
    

	void Awake()
	{
		img = new Image[2];
		img[0] = box.GetComponent<Image>();
		img[1] = arrow.GetComponent<Image>();
		box.sizeDelta = new Vector2(maxWidth, box.sizeDelta.y);
		BGColorFade = BGColor;
		BGColorFade.a = 0;
		textColorFade = textColor;
		textColorFade.a = 0;
		isUI = false;
		foreach(Image bg in img)
		{
			bg.color = BGColorFade;
		}
		boxText.color = textColorFade;
		boxText.alignment = TextAnchor.MiddleCenter;
	}

	void LateUpdate()
	{
		bool show = false;
		boxText.fontSize = fontSize;

		if(tooltipMode == ProjectMode.Tooltip3D)
		{
			RaycastHit hit;
			Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hit))
			{
				if(hit.transform.GetComponent<ToolTipText>() )
				{
					text = hit.transform.GetComponent<ToolTipText>().text;
					show = true;
				}
			}
		}
		else
		{
			RaycastHit2D hit = Physics2D.Raycast(_camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			if(hit.transform != null)
			{
				if(hit.transform.GetComponent<ToolTipText>() && hit.transform.GetComponent<CurrentItem>().inventory.items[hit.transform.GetComponent<CurrentItem>().index].id != 0)
				{
					text = hit.transform.GetComponent<ToolTipText>().text;
					show = true;
				}
			}
		}

		boxText.text = text;
		float width = maxWidth;
		if(boxText.preferredWidth <= maxWidth - border) width = boxText.preferredWidth + border;
		box.sizeDelta = new Vector2(width, boxText.preferredHeight + border);

		float arrowShift = width / 4; // сдвиг позиции стрелки по Х

		if(show || isUI)
		{
			float arrowPositionY = -(arrow.sizeDelta.y / 2 - 1); // позиция стрелки по умолчанию (внизу)
			float arrowPositionX = arrowShift;
			
			float curY = Input.mousePosition.y + box.sizeDelta.y / 2 + arrow.sizeDelta.y;
			Vector3 arrowScale = new Vector3(1, 1, 1);
			if(curY + box.sizeDelta.y / 2 > Screen.height) // если Tooltip выходит за рамки экрана, в данном случаи по высоте
			{
				curY = Input.mousePosition.y - box.sizeDelta.y / 2 - arrow.sizeDelta.y;
				arrowPositionY = box.sizeDelta.y + (arrow.sizeDelta.y / 2 - 1);
				arrowScale = new Vector3(1, -1, 1); // отражение по вертикале
			}
			
			float curX = Input.mousePosition.x + arrowShift;
			if(curX + box.sizeDelta.x / 2 > Screen.width)
			{
				curX = Input.mousePosition.x - arrowShift;
				arrowPositionX = width - arrowShift;
			}
			
			box.anchoredPosition = new Vector2(curX, curY);
			
			arrow.anchoredPosition = new Vector2(arrowPositionX, arrowPositionY);
			arrow.localScale = arrowScale;

			foreach(Image bg in img)
			{
				bg.color = Color.Lerp(bg.color, BGColor, speed * Time.deltaTime);
			}
			boxText.color = Color.Lerp(boxText.color, textColor, speed * Time.deltaTime);
		}
		else
		{
            foreach(Image bg in img)
			{
				bg.color = Color.Lerp(bg.color, BGColorFade, speed * Time.deltaTime);
			}
			boxText.color = Color.Lerp(boxText.color, textColorFade, speed * Time.deltaTime);
		}
	}
}
