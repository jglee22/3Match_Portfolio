using UnityEngine;

public static class MaskLoader
{
    // CSV 파일을 bool[,] 그리드 마스크로 변환하는 유틸 함수
    public static bool[,] LoadMaskFromCSV(TextAsset csv)
    {
        if (csv == null)
        {
            Debug.LogError("[MaskLoader] CSV 파일이 null입니다.");
            return null;
        }

        string[] lines = csv.text.Trim().Split('\n');
        int height = lines.Length;
        int width = lines[0].Split(',').Length;

        bool[,] mask = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            string[] row = lines[y].Trim().Split(',');

            for (int x = 0; x < width; x++)
            {
                // Unity 기준으로 아래에서 위로 가도록 y 인덱스 반전
                int reversedY = height - 1 - y;
                mask[x, reversedY] = row[x].Trim() == "1";
            }
        }

        return mask;
    }
}
