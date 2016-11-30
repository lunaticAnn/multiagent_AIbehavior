using UnityEngine;
using System.Collections;
using System;


/// <summary>
/// Omg they don't have heap.
/// </summary>
public class Heap<T> where T:IHeapItem<T>{

	T[] items;
	int current_count;

	public Heap(int max_heapsize){
		items=new T[max_heapsize];
	}

	public void Add(T item){
		item.heap_index=current_count;
		items[current_count]=item;
		sort_up(item);
		current_count++;
	}

	public T remove_first(){
		T first_item=items[0];
		current_count--;
		items[0]=items[current_count];
		items[0].heap_index=0;
		sort_down(items[0]);
		return first_item;
	}

	public int count{
		get{
			return current_count;
		}
	}

	public void update_item(T item){
		sort_up(item);  
	}

	public bool Contains(T item){
		return Equals(items[item.heap_index],item);
	}

	void sort_down(T item){
		while(true){
			int child_index_left = item.heap_index * 2 + 1;
			int child_index_right = item.heap_index * 2 + 2;
			int swap_index = 0;
			if(child_index_left<current_count){
				swap_index=child_index_left;
				if(child_index_right<current_count){
					if (items[child_index_left].CompareTo(items[child_index_right])<0){
						swap_index=child_index_right;
					}
				}

				if(item.CompareTo(items[swap_index])<0){
					Swap(item,items[swap_index]);
				}
				else{
					return;
				}
			}
			else{
				return;
			}
		}
	}

	void sort_up(T item){
		int parent_index = (item.heap_index-1)/2;

		while(true){
			T parent_item = items[parent_index];
			if(item.CompareTo(parent_item)>0){
				Swap(item,parent_item);
			}
			else{
				break;
			}

			parent_index=(item.heap_index-1)/2;
		}
	}

	void Swap(T itemA, T itemB){
		items[itemA.heap_index]=itemB;
		items[itemB.heap_index]=itemA;
		int temp_indexA=itemA.heap_index;
		itemA.heap_index=itemB.heap_index;
		itemB.heap_index=temp_indexA;
	}

}

public interface IHeapItem<T>:IComparable<T>{
	int heap_index{ //how to use this interface???
		get;
		set;
	}
		
}
