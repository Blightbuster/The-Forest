using System;
using System.Collections.Generic;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	[AddComponentMenu("Buildings/Creation/Roof Architect")]
	[DoNotSerializePublic]
	public class RoofArchitect2 : RoofArchitect
	{
		protected override void SpawnRoof()
		{
			float num = float.MaxValue;
			float num2 = float.MinValue;
			float num3 = float.MaxValue;
			float num4 = float.MinValue;
			Vector3 forward = this._multiPointsPositions[1] - this._multiPointsPositions[0];
			base.transform.rotation = Quaternion.LookRotation(forward);
			Vector3[] array = (from p in this._multiPointsPositions
			select base.transform.InverseTransformPoint(p)).ToArray<Vector3>();
			float y = array[0].y;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].y = 0f;
				if (array[i].z < num)
				{
					num = array[i].z;
				}
				if (array[i].z > num2)
				{
					num2 = array[i].z;
				}
				if (array[i].x < num3)
				{
					num3 = array[i].x;
				}
				if (array[i].x > num4)
				{
					num4 = array[i].x;
				}
			}
			float num5 = 0.65f * this.RowWidth;
			float rowWidth = this.RowWidth;
			for (int j = 0; j < array.Length; j++)
			{
				if (Mathf.Abs(array[j].z - num) < rowWidth)
				{
					Vector3[] array2 = array;
					int num6 = j;
					array2[num6].z = array2[num6].z - num5;
				}
				else if (Mathf.Abs(array[j].z - num2) < rowWidth)
				{
					Vector3[] array3 = array;
					int num7 = j;
					array3[num7].z = array3[num7].z + num5;
				}
				if (Mathf.Abs(array[j].x - num3) < rowWidth)
				{
					Vector3[] array4 = array;
					int num8 = j;
					array4[num8].x = array4[num8].x - num5;
				}
				else if (Mathf.Abs(array[j].x - num4) < rowWidth)
				{
					Vector3[] array5 = array;
					int num9 = j;
					array5[num9].x = array5[num9].x + num5;
				}
			}
			num -= num5;
			num2 += num5;
			float num10 = Mathf.Abs(num2 - num);
			this._rowCount = Mathf.CeilToInt(num10 / this.RowWidth);
			float num11 = Mathf.Clamp(this._roofHeight, 0f, (float)this._rowCount / 2f * (base.LogWidth / 2f));
			base.InitRowPointsBuffer(this._rowCount, this._rowPointsBuffer, out this._rowPointsBuffer);
			base.CalcRowPointBufferForArray(array, num, this._rowPointsBuffer);
			float num12 = float.MinValue;
			float num13 = float.MaxValue;
			List<Vector3>[] array6 = new List<Vector3>[this._rowPointsBuffer.Length];
			for (int k = 0; k < this._rowCount; k++)
			{
				this._rowPointsBuffer[k].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
				array6[k] = new List<Vector3>();
				int count = this._rowPointsBuffer[k].Count;
				for (int l = 1; l < count; l += 2)
				{
					Vector3 a = this._rowPointsBuffer[k][l - 1];
					Vector3 vector = this._rowPointsBuffer[k][l];
					array6[k].Add(Vector3.Lerp(a, vector, 0.5f));
					vector.y = this._logWidth;
					this._rowPointsBuffer[k][l] = vector;
				}
				for (int m = 1; m < count; m += 2)
				{
					int num14 = 0;
					while (m < count && this._rowPointsBuffer[k][m].y < 0f)
					{
						num14 += 2;
						m += 2;
					}
					if (m < count)
					{
						float num15 = Mathf.Abs(this._rowPointsBuffer[k][m - 1 - num14].x - this._rowPointsBuffer[k][m].x);
						if (num15 > num12)
						{
							num12 = num15;
						}
						if (num15 < num13)
						{
							num13 = num15;
						}
					}
				}
			}
			float num16 = 1f - num13 / num12;
			if (this._holes != null)
			{
				for (int n = 0; n < this._holes.Count; n++)
				{
					Vector3[] array7 = new Vector3[5];
					Hole hole = this._holes[n];
					hole._used = false;
					array7[0] = base.transform.InverseTransformPoint(hole._position + new Vector3(hole._size.x, 0f, hole._size.y).RotateY(hole._yRotation));
					array7[1] = base.transform.InverseTransformPoint(hole._position + new Vector3(hole._size.x, 0f, -hole._size.y).RotateY(hole._yRotation));
					array7[2] = base.transform.InverseTransformPoint(hole._position + new Vector3(-hole._size.x, 0f, -hole._size.y).RotateY(hole._yRotation));
					array7[3] = base.transform.InverseTransformPoint(hole._position + new Vector3(-hole._size.x, 0f, hole._size.y).RotateY(hole._yRotation));
					array7[4] = array7[0];
					for (int num17 = 0; num17 < 5; num17++)
					{
						array7[num17].y = array[0].y;
					}
					base.InitRowPointsBuffer(this._rowCount, this._holesRowPointsBuffer, out this._holesRowPointsBuffer);
					base.CalcRowPointBufferForArray(array7, num, this._holesRowPointsBuffer);
					for (int num18 = 0; num18 < this._rowCount; num18++)
					{
						this._rowPointsBuffer[num18].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
						this._holesRowPointsBuffer[num18].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
						List<Vector3> list = this._rowPointsBuffer[num18];
						List<Vector3> list2 = this._holesRowPointsBuffer[num18];
						int num19 = list.Count;
						int count2 = list2.Count;
						for (int num20 = 1; num20 < num19; num20 += 2)
						{
							int num21 = num20 - 1;
							while (num21 > 1 && this._rowPointsBuffer[num18][num21].y < 0f)
							{
								num21 -= 2;
							}
							int num22 = num20;
							while (num22 + 2 < num19 && this._rowPointsBuffer[num18][num22].y < 0f)
							{
								num22 += 2;
							}
							bool flag = num20 - 1 == num21;
							bool flag2 = num20 == num22;
							float x = this._rowPointsBuffer[num18][num21].x;
							float x5 = this._rowPointsBuffer[num18][num22].x;
							float num23 = Mathf.Abs(x - x5);
							float num24 = Mathf.Lerp(x, x5, Mathf.Abs(list[num20 - 1].y));
							float num25 = Mathf.Lerp(x, x5, Mathf.Abs(list[num20].y));
							for (int num26 = 1; num26 < count2; num26 += 2)
							{
								float x3 = list2[num26 - 1].x;
								float x4 = list2[num26].x;
								if (num24 > x3 && num25 < x4)
								{
									if (num19 > 2 && (flag2 || flag) && (!flag2 || !flag))
									{
										if (flag)
										{
											list.RemoveAt(num20);
											Vector3 value = list[num20 - 1];
											value.y = Mathf.Abs(list[num20].y);
											list[num20 - 1] = value;
											list.RemoveAt(num20);
										}
										else
										{
											Vector3 value2 = list[num20];
											value2.y = Mathf.Abs(list[num20 - 1 - 1].y);
											list[num20] = value2;
											list.RemoveAt(num20 - 1);
											list.RemoveAt(num20 - 1 - 1);
										}
									}
									else
									{
										list.RemoveAt(num20);
										list.RemoveAt(num20 - 1);
									}
									hole._used = true;
									if (num26 + 2 >= count2)
									{
										if (num20 + 2 >= num19)
										{
											break;
										}
										num20 -= 2;
										num19 -= 2;
									}
								}
								else if (num24 < x3 && num25 > x4)
								{
									Vector3 item = list2[num26 - 1];
									item.y = -Mathf.Abs(x3 - x) / num23;
									Vector3 item2 = list2[num26];
									item2.y = -Mathf.Abs(x4 - x) / num23;
									list.Insert(num20, item);
									list.Insert(num20 + 1, item2);
									hole._used = true;
									num20 += 2;
									num19 += 2;
								}
								else if (num24 > x3 && num24 < x4)
								{
									Vector3 value3 = list[num20 - 1];
									value3.y = Mathf.Abs(x4 - x) / num23;
									if (!flag)
									{
										value3.x = x4;
										value3.y = -value3.y;
									}
									list[num20 - 1] = value3;
									hole._used = true;
								}
								else if (num25 > x3 && num25 < x4)
								{
									Vector3 value4 = list[num20];
									value4.y = Mathf.Abs(x3 - x) / num23;
									if (!flag2)
									{
										value4.x = x3;
										value4.y = -value4.y;
									}
									list[num20] = value4;
									hole._used = true;
								}
							}
						}
					}
				}
			}
			Transform roofRoot = this._roofRoot;
			float num27 = 0f;
			float logStackScaleRatio = (this._maxScaleLogCost <= 0f) ? 0f : (this._maxLogScale / this._maxScaleLogCost);
			for (int num28 = 0; num28 < this._rowCount; num28++)
			{
				int count3 = this._rowPointsBuffer[num28].Count;
				for (int num29 = 1; num29 < count3; num29 += 2)
				{
					int num30 = num29 - 1;
					while (num30 > 1 && this._rowPointsBuffer[num28][num30].y < 0f)
					{
						num30 -= 2;
					}
					int num31 = num29;
					while (num31 + 2 < count3 && this._rowPointsBuffer[num28][num31].y < 0f)
					{
						num31 += 2;
					}
					int num32 = 0;
					while (num32 + 1 < array6[num28].Count && this._rowPointsBuffer[num28][num30].x > array6[num28][num32].x)
					{
						num32++;
					}
					Vector3 vector2 = this._rowPointsBuffer[num28][num29 - 1];
					Vector3 vector3 = this._rowPointsBuffer[num28][num29];
					Vector3 vector4 = this._rowPointsBuffer[num28][num30];
					Vector3 vector5 = this._rowPointsBuffer[num28][num31];
					float y2 = vector2.y;
					float y3 = vector3.y;
					float num33 = Mathf.Abs(y2) * 2f - 1f;
					float num34 = Mathf.Abs(y3) * 2f - 1f;
					float num35 = Mathf.Abs(vector4.x - vector5.x);
					vector4.y = y;
					vector5.y = y;
					vector4 = base.transform.TransformPoint(vector4);
					vector5 = base.transform.TransformPoint(vector5);
					Vector3 vector6 = array6[num28][num32];
					float num36 = 1f - Mathf.Abs(((float)num28 / (float)this._rowCount - 0.5f) * 2f);
					num36 = Mathf.InverseLerp(0.15f, 0.85f, num36);
					vector6.y = y + num36 * num11;
					vector6 = base.transform.TransformPoint(vector6);
					if ((num33 > 0f && num34 > 0f) || (num33 < 0f && num34 < 0f))
					{
						if (num33 > 0f)
						{
							Vector3 vector7 = Vector3.Lerp(vector6, vector5, Mathf.Abs(num33));
							Vector3 a2 = Vector3.Lerp(vector6, vector5, Mathf.Abs(num34));
							base.SpawnChunk(a2 - vector7, vector7, roofRoot, logStackScaleRatio, ref num27);
						}
						else if (num33 < 0f)
						{
							Vector3 vector8 = Vector3.Lerp(vector6, vector4, Mathf.Abs(num33));
							Vector3 a3 = Vector3.Lerp(vector6, vector4, Mathf.Abs(num34));
							base.SpawnChunk(a3 - vector8, vector8, roofRoot, logStackScaleRatio, ref num27);
						}
					}
					else
					{
						Vector3 vector9 = Vector3.Lerp(vector6, vector4, Mathf.Abs(num33));
						Vector3 a4 = vector6;
						base.SpawnChunk(a4 - vector9, vector9, roofRoot, logStackScaleRatio, ref num27);
						vector9 = Vector3.Lerp(vector6, vector5, Mathf.Abs(num34));
						base.SpawnChunk(a4 - vector9, vector9, roofRoot, logStackScaleRatio, ref num27);
					}
				}
			}
		}

		public override float RowWidth
		{
			get
			{
				return this._logWidth * 0.9f;
			}
		}

		public override float RowChunkLengthRatio
		{
			get
			{
				return 1.04f;
			}
		}
	}
}
