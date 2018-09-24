//
//  WindowController.m
//  Zebra Emulator
//
//  Created by Daniel Hazelbaker on 9/24/18.
//  Copyright © 2018 Daniel Hazelbaker. All rights reserved.
//

#import "WindowController.h"
#import "ViewController.h"

@interface WindowController ()

@property (strong, nonatomic) IBOutlet NSPopUpButtonCell *widthMenuCell;
@property (strong, nonatomic) IBOutlet NSPopUpButtonCell *heightMenuCell;

@end

@implementation WindowController

- (void)windowDidLoad
{
    NSUserDefaults *defaults = NSUserDefaults.standardUserDefaults;

    [super windowDidLoad];
    
    //
    // Restore the label width.
    //
    NSString *menuTitle = [defaults stringForKey:@"DefaultLabelWidth"];
    if (menuTitle == nil)
    {
        menuTitle = @"4.00\"";
    }
    [self.widthMenuCell selectItem:[self.widthMenuCell itemWithTitle:menuTitle]];

    //
    // Restore the label height.
    //
    menuTitle = [defaults stringForKey:@"DefaultLabelHeight"];
    if (menuTitle == nil)
    {
        menuTitle = @"2.00\"";
    }
    [self.heightMenuCell selectItem:[self.heightMenuCell itemWithTitle:menuTitle]];

    //
    // Set initial label size.
    //
    [((ViewController *)self.contentViewController) updateLabelWidth:[self.widthMenuCell.selectedItem.title floatValue] height:[self.heightMenuCell.selectedItem.title floatValue]];
}

- (IBAction)btnClearLabels:(id)sender
{
    [((ViewController *)self.contentViewController) clearLabels];
}

- (IBAction)updateLabelSize:(id)sender
{
    [((ViewController *)self.contentViewController) updateLabelWidth:[self.widthMenuCell.selectedItem.title floatValue] height:[self.heightMenuCell.selectedItem.title floatValue]];
    
    NSUserDefaults *defaults = NSUserDefaults.standardUserDefaults;
    [defaults setObject:self.widthMenuCell.selectedItem.title forKey:@"DefaultLabelWidth"];
    [defaults setObject:self.heightMenuCell.selectedItem.title forKey:@"DefaultLabelHeight"];
}

@end
