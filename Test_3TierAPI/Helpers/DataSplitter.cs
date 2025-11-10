namespace Test_3TierAPI.Helpers
{
    public static class DataSplitter
    {
        /// <summary>
        /// 리스트를 지정된 크기만큼 분할하여 List<List<T>>로 반환
        /// </summary>
        /// <typeparam name="T">리스트의 항목 타입</typeparam>
        /// <param name="sourceList">원본 리스트</param>
        /// <param name="splitCount">분할할 항목 개수 (한 그룹당)</param>
        /// <returns>List<List<T>> 형태로 분할된 리스트 그룹</returns>
        public static List<List<T>> SplitList<T>(List<T> sourceList, int splitCount)
        {
            if (sourceList == null)
                throw new ArgumentNullException(nameof(sourceList));

            if (splitCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(splitCount), "splitCount는 1 이상이어야 합니다.");

            List<List<T>> result = new List<List<T>>();

            for (int i = 0; i < sourceList.Count; i += splitCount)
            {
                int count = Math.Min(splitCount, sourceList.Count - i);
                result.Add(sourceList.GetRange(i, count));
            }

            return result;
        }
    }
}
