import { CommonModule} from '@angular/common';
import { Component,EventEmitter,Input,Output,OnDestroy,OnInit  } from '@angular/core';
import { debounceTime, distinctUntilChanged, Subject, Subscription } from 'rxjs';

@Component({
  selector: 'app-generic-search',
  imports: [CommonModule],
  templateUrl: './generic-search.html',
  styleUrl: './generic-search.css',
})
export class GenericSearch {

  @Input() placeholder: string = 'Search...';
  @Input() debounceTime: number = 300;
  @Output() onSearch = new EventEmitter<string>();

  private searchSubject = new Subject<string>();
  private searchSubscription?:Subscription;
  
  ngOnInit() {
    this.searchSubscription = this.searchSubject
      .pipe(debounceTime(this.debounceTime), distinctUntilChanged())
      .subscribe(searchTerm => {
        this.onSearch.emit(searchTerm);
      });
  }

  handleInput(event:Event):void{
    const target = event.target as HTMLInputElement;
    this.searchSubject.next(target.value);
  }

  clearSearch(input: HTMLInputElement): void {
    input.value = '';
    this.searchSubject.next('');
  }

  ngOnDestroy():void{
    this.searchSubscription?.unsubscribe();
  }

}
